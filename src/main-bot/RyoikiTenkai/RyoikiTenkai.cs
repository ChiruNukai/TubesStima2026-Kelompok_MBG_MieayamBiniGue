using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Ryōiki Tenkai🤞
// ------------------------------------------------------------------
// Penargetan: Play It Forward
// Pergerakan: Anti-Gravity & Stop and Go
// ------------------------------------------------------------------

// ------------------------------------------------------------------
public class RyoikiTenkai : Bot{
    // Kumpulan konstanta yang digunakan untuk mengatur
    // behavior movement, targeting, dan anti-gravity bot.

    // Threshold minimum energi musuh agar dianggap berbahaya
    // pada perhitungan anti-gravity.
    private readonly static double ENEMY_ENERGY_THRESHOLD = 1.3;

    // Threshold agresif bot
    private readonly static double AGGRESSIVE_DISTANCE_TARGET = 150;
    private readonly static double AGGRESSIVE_FIREPOWER_BONUS = 1.5;
    private readonly static int AGGRESSIVE_CLOSE_SPEED = 6;

    // Jarak aman minimum dari tembok arena.
    // Bot akan menghindari titik yang terlalu dekat dinding.
    private readonly static double WALL_SAFE_MARGIN = 25;

    // Faktor pengali firepower berdasarkan jarak musuh.
    // Semakin besar nilainya, semakin agresif bot menembak.
    private readonly static double FIREPOWER_MULTIPLIER = 5;

    // Minimum energi bot untuk tetap boleh menembak.
    // Mencegah bot kehabisan energi terlalu cepat.
    private readonly static double MINIMUM_FIRE_ENERGY = 12;

    // Batas heat radar untuk melakukan radar lock.
    // Digunakan agar radar tetap fokus ke target.
    private readonly static double RADAR_LOCK_THRESHOLD = 0.7;

    // Radius minimum titik movement anti-gravity.
    // Bot tidak memilih titik yang terlalu dekat.
    private readonly static double MINIMUM_MOVEMENT_RADIUS = 200;

    // Radius maksimum pencarian titik anti-gravity.
    private readonly static double MAXIMUM_MOVEMENT_RADIUS = 300;

    // Jumlah titik kandidat movement yang dicek.
    // Semakin besar nilainya, movement makin halus
    // tapi perhitungan makin berat.
    private readonly static double MOVEMENT_SAMPLE_POINTS = 36;

    // Nilai kecil untuk mencegah pembagian dengan nol.
    private readonly static double EPSILON = 1e-6;

    // Batas maksimum hit sebelum mode stop-and-go dimatikan.
    private readonly static int STOP_AND_GO_HIT_LIMIT = 3;

    // Jumlah state yang digunakan pada N-Gram prediction.
    // Semakin besar nilainya, prediksi lebih spesifik.
    private readonly static int NGRAM_SEQUENCE_LENGTH = 4;

    // Margin tambahan agar virtual bullet dianggap keluar arena.
    private readonly static int BULLET_ARENA_OFFSET = 50;

    // Besarnya gaya tolak dari musuh pada anti-gravity.
    private readonly static int ENEMY_REPULSION_FORCE = 300;

    // Besarnya gaya tolak dari virtual bullet.
    private readonly static int BULLET_REPULSION_FORCE = 10;

    // Gravity kecil agar bot tidak diam di lokasi yang sama terus.
    private readonly static int LAST_POSITION_REPULSION = 10;

    // Gravity untuk menghindari sudut arena.
    // Sudut berbahaya karena movement lebih terbatas.
    private readonly static int CORNER_REPULSION_FORCE = 100;

    // ID target utama yang sedang difokuskan bot.
    static int currentTargetId;

    // Jarak target utama saat ini.
    static double currentTargetDistance;

    // Jarak musuh terakhir yang berhasil discan radar.
    static double scannedEnemyDistance;

    // Titik tujuan movement anti-gravity.
    // Bot akan bergerak menuju koordinat ini.
    static double destinationX;
    static double destinationY;

    // Variabel stop-and-go movement.
    // movementDirection = arah orbit movement.
    // stopAndGoHitCount = jumlah hit yang diterima saat movementDirection  aktif.
    // disableStopAndGo = flag untuk mematikan movementDirection  jika terlalu berbahaya.
    static int movementDirection = 1;
    static int stopAndGoHitCount;
    static bool disableStopAndGo;

    // Digunakan untuk menghasilkan angka random.
    // Dipakai untuk variasi warna dan movement bot.
    // tidak terlalu mudah diprediksi.
    Random rand = new Random();

    // Menyimpan seluruh data musuh berdasarkan ID.
    // Berisi histori movement, energy, prediction, dll.
    static Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>();

    // Menyimpan virtual bullet musuh.
    // Digunakan untuk simulasi dodge peluru.
    static List<Bullet> bullets;

    // Menyimpan virtual bullet milik bot sendiri.
    // Digunakan untuk mengecek efektivitas targeting.
    static List<MyBullet> myBullets;

    static void Main(){
        new RyoikiTenkai().Start();
    }

    RyoikiTenkai() : base(BotInfo.FromFile("RyoikiTenkai.json")) { }

    // Method pertama saat ronde dimulai.
    // Digunakan untuk reset data dan setup radar.
    public override void Run(){
        Console.WriteLine("🤞Ryōiki Tenkai🤞 | Round : " + RoundNumber);

        // Warna bot.
        RadarColor = Color.White;
        TracksColor = Color.White;
        GunColor = Color.White;

        // Radar berputar terus mencari musuh.
        SetTurnRadarRight(double.PositiveInfinity);

        // Radar dan gun tidak ikut body turn.
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        // Reset target dan bullet.
        currentTargetDistance = double.PositiveInfinity;
        scannedEnemyDistance = double.PositiveInfinity;

        bullets = new List<Bullet>();
        myBullets = new List<MyBullet>();

        // Reset stop-and-go.
        disableStopAndGo = false;
        stopAndGoHitCount = 0;
    }

    // Method yang dijalankan setiap tick/frame.
    // Digunakan untuk update bullet dan movement.
    public override void OnTick(TickEvent e){
        // Warna bot.
        TurretColor = Color.Red;
        ScanColor = Color.DarkBlue;
        BodyColor = ScanColor;
        BulletColor = ScanColor;

        var g = Graphics;

        for (int i = bullets.Count - 1; i >= 0; i--){
            Bullet bullet = bullets[i];

            // Update posisi bullet.
            bullet.X += bullet.Speed * Math.Cos(bullet.Direction);
            bullet.Y += bullet.Speed * Math.Sin(bullet.Direction);

            // Gambar virtual bullet.
            g.FillRectangle(Brushes.Black, (float)bullet.X, (float)bullet.Y, (float)(3 * bullet.Power), (float)(3 * bullet.Power));

            // Hapus bullet yang keluar arena.
            if (bullet.X < 0 - BULLET_ARENA_OFFSET || bullet.X > ArenaWidth + BULLET_ARENA_OFFSET ||
                bullet.Y < 0 - BULLET_ARENA_OFFSET || bullet.Y > ArenaHeight + BULLET_ARENA_OFFSET){
                bullets.RemoveAt(i);
            }
            else{
                bullets[i] = bullet;
            }
        }

        for (int i = myBullets.Count - 1; i >= 0; i--){
            Bullet bullet = myBullets[i].BulletData;

            // Update posisi bullet.
            bullet.X += bullet.Speed * Math.Cos(bullet.Direction);
            bullet.Y += bullet.Speed * Math.Sin(bullet.Direction);

            // Hitam = head-on, merah = prediction.
            g.FillRectangle(myBullets[i].Type == 0 ? Brushes.Black : Brushes.Red, (float)bullet.X, (float)bullet.Y, (float)(3 * bullet.Power), (float)(3 * bullet.Power));


            EnemyData data = enemyData[myBullets[i].Target];

            // Bullet dianggap hit.
            if (distance(data.LastX, data.LastY, bullet.X, bullet.Y) < 18){
                data.Type[myBullets[i].Type] += 5;
                myBullets.RemoveAt(i);
            }

            // Bullet dianggap miss.
            else if (bullet.X < 0 - BULLET_ARENA_OFFSET || bullet.X > ArenaWidth + BULLET_ARENA_OFFSET ||
                bullet.Y < 0 - BULLET_ARENA_OFFSET || bullet.Y > ArenaHeight + BULLET_ARENA_OFFSET){
                data.Type[myBullets[i].Type]--;
                myBullets.RemoveAt(i);
            }
            else{
                myBullets[i].BulletData = bullet;
            }
        }

        // Matikan stop-and-go jika terlalu sering kena hit.
        if (stopAndGoHitCount > STOP_AND_GO_HIT_LIMIT)
            disableStopAndGo = true;

        // Stop-and-go aktif saat musuh jauh.
        if (!disableStopAndGo && EnemyCount == 1 && currentTargetDistance > 250) return;

        double bestX = X;
        double bestY = Y;
        double minGrav = double.PositiveInfinity;

        // Cari titik movement terbaik.
        for (int i = 0; i < MOVEMENT_SAMPLE_POINTS; i++){
            double theta = (2 * Math.PI / MOVEMENT_SAMPLE_POINTS) * i;

            for (int u = 0; u <= 1; u++){
                // Radius candidate point.
                double r = Math.Sqrt(u * (MAXIMUM_MOVEMENT_RADIUS * MAXIMUM_MOVEMENT_RADIUS - MINIMUM_MOVEMENT_RADIUS * MINIMUM_MOVEMENT_RADIUS) + MINIMUM_MOVEMENT_RADIUS * MINIMUM_MOVEMENT_RADIUS);

                // Posisi candidate point.
                double x = X + r * Math.Cos(theta);
                double y = Y + r * Math.Sin(theta);

                // Skip titik dekat tembok.
                if (x < WALL_SAFE_MARGIN || x > ArenaWidth - WALL_SAFE_MARGIN ||
                    y < WALL_SAFE_MARGIN || y > ArenaHeight - WALL_SAFE_MARGIN){
                    continue;
                }

                // Hitung danger level titik.
                double grav = CalcGrav(x, y);

                // Pilih titik paling aman.
                if (grav < minGrav){
                    minGrav = grav;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        // Update destination jika lebih aman.
        if (minGrav < CalcGrav(destinationX, destinationY) * 0.9){
            destinationX = bestX;
            destinationY = bestY;
        }

        // Arah menuju destination.
        double turn = BearingTo(destinationX, destinationY) * Math.PI / 180;

        // Tangent movement agar tidak linear.
        SetTurnLeft(180 / Math.PI * Math.Tan(turn));

        // Bergerak menuju destination.
        SetForward(DistanceTo(destinationX, destinationY) * Math.Cos(turn));
    }

    // Method yang dijalankan saat radar menemukan musuh.
    // Digunakan untuk targeting, prediction, dan firing.
    public override void OnScannedBot(ScannedBotEvent e){
        // Tambahkan enemy baru jika belum ada.
        if (!enemyData.ContainsKey(e.ScannedBotId)){
            enemyData[e.ScannedBotId] = new EnemyData();
        }
        EnemyData data = enemyData[e.ScannedBotId];

        // Simpan posisi terakhir musuh.
        data.LastX = e.X;
        data.LastY = e.Y;
        data.IsAlive = true;

        // Ambil jarak musuh hasil scan.
        double scannedDistance = scannedEnemyDistance = DistanceTo(e.X, e.Y);

        // Prioritaskan musuh terdekat.
        if (scannedDistance < currentTargetDistance){
            currentTargetId = e.ScannedBotId;
        }

        // Ignore target lain saat gun masih panas.
        else if (e.ScannedBotId != currentTargetId && GunHeat != 0){
            return;
        }
        currentTargetDistance = scannedDistance;


        // Radar tetap fokus ke target.
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        if (!double.IsNaN(radarAngle) && (GunHeat < RADAR_LOCK_THRESHOLD || EnemyCount == 1)){
            SetTurnRadarLeft(radarAngle);
        }

        // Power peluru berdasarkan jarak musuh.
        // Power peluru berdasarkan jarak musuh.
        double firePower = Energy / DistanceTo(e.X, e.Y) * FIREPOWER_MULTIPLIER;

        if (EnemyCount == 1){
            firePower *= AGGRESSIVE_FIREPOWER_BONUS;
        }

        firePower = Math.Min(firePower, 3.0); // Cap at max allowed power

        // Menembak jika gun siap.
        if (GunTurnRemaining == 0 && (Energy > MINIMUM_FIRE_ENERGY || DistanceTo(e.X, e.Y) < 50)){
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);

        // Ubah arah musuh ke radian.
        double currentDirection = e.Direction * Math.PI / 180.0;

        // Energy drop menandakan musuh menembak.
        double energyDrop = data.LastEnergy - e.Energy;

        if (EnemyCount == 1 && 0.11 < energyDrop && energyDrop <= 3){
            if (!disableStopAndGo && DistanceRemaining == 0){
                // Ganti arah jika dekat tembok.
                if (X < WALL_SAFE_MARGIN || X > ArenaWidth - WALL_SAFE_MARGIN ||
                    Y < WALL_SAFE_MARGIN || Y > ArenaHeight - WALL_SAFE_MARGIN){
                    movementDirection = -movementDirection;
                    stopAndGoHitCount = 0;
                }

                if (currentTargetDistance > AGGRESSIVE_DISTANCE_TARGET){
                    // ── AGGRESSIVE MODE ──
                    // Close in on the enemy while still weaving sideways.
                    double aggrAngle = (BearingTo(e.X, e.Y) +
                                        (45 * movementDirection)) * Math.PI / 180;

                    SetTurnLeft(Math.Tan(aggrAngle) * 180 / Math.PI);

                    // Drive forward hard to close the gap.
                    SetForward(AGGRESSIVE_CLOSE_SPEED * 8 * Math.Sign(Math.Cos(aggrAngle)));
                }
                else{
                    // ── STOP-AND-GO MODE (already in range) ──
                    // Tight orbit + speed burst timed to enemy shot.
                    double turn = (BearingTo(e.X, e.Y) +
                                (90 - 15 * (currentTargetDistance / 1000)) * movementDirection)
                                * Math.PI / 180;

                    SetTurnLeft(Math.Tan(turn) * 180 / Math.PI);
                    SetForward((3 + (int)(energyDrop * 1.999999)) * 8 * Math.Sign(Math.Cos(turn)));
                }
            }
        }

        // Simpan energy terakhir musuh.
        data.LastEnergy = e.Energy;

        // Speed musuh saat ini.
        double currentSpeed = e.Speed;

        // Hitung acceleration musuh.
        double acceleration = data.HasPrevious ? currentSpeed - data.LastSpeed : 0;
        data.LastSpeed = currentSpeed;

        // Hitung angular velocity musuh.
        double angularVelocity = data.HasPrevious ? (currentDirection - data.LastDirection + Math.PI) % (2 * Math.PI) - Math.PI : 0;
        data.LastDirection = currentDirection;

        // Simpan state movement musuh.
        State currentState = new State(angularVelocity, currentSpeed, acceleration);
        data.StateHistory.Add(currentState);

        if (data.StateHistory.Count >= NGRAM_SEQUENCE_LENGTH){
            List<State> contextStates = data.StateHistory.GetRange(data.StateHistory.Count - (NGRAM_SEQUENCE_LENGTH - 1), NGRAM_SEQUENCE_LENGTH - 1);
            StateSequence contextKey = new StateSequence(contextStates);
            if (!data.NgramTree.ContainsKey(contextKey))
            {
                data.NgramTree[contextKey] = new TransitionSegmentTree();
            }

            // Tambahkan transisi state.
            data.NgramTree[contextKey].Add(currentState);
        }
        data.HasPrevious = true;

        // Gunakan head-on jika prediction kurang akurat.
        if (data.Type.IndexOf(data.Type.Max()) != 0){
            SetTurnGunLeft(GunBearingTo(e.X, e.Y));
            return;
        }

        // Prediksi posisi masa depan musuh.
        double predictedX = e.X;
        double predictedY = e.Y;
        double predictedDirection = currentDirection;
        double predictedSpeed = currentSpeed;
        double simAngularVelocity = angularVelocity;
        State simCurrentState = currentState;
        int time = 0;

        List<State> simContext = null;

        // Ambil context prediction.
        if (data.StateHistory.Count >= NGRAM_SEQUENCE_LENGTH - 1){
            simContext = new List<State>(data.StateHistory.GetRange(data.StateHistory.Count - (NGRAM_SEQUENCE_LENGTH - 1), NGRAM_SEQUENCE_LENGTH - 1));
        }

        // Simulasi movement musuh.
        while (time * bulletSpeed < DistanceTo(predictedX, predictedY) && time < 100){
            if (simContext != null){
                StateSequence simContextKey = new StateSequence(simContext);

                // Prediksi next state dari N-Gram.
                if (data.NgramTree.ContainsKey(simContextKey)){
                    State nextState = data.NgramTree[simContextKey].GetMostFrequent();
                    simAngularVelocity = nextState.AngularVelocity / 1024.0;
                    predictedSpeed += nextState.Acceleration;
                    simContext.RemoveAt(0);
                    simContext.Add(nextState);
                }
            }

            // Update posisi prediksi.
            predictedDirection += simAngularVelocity;
            predictedX += predictedSpeed * Math.Cos(predictedDirection);
            predictedY += predictedSpeed * Math.Sin(predictedDirection);
            time++;
        }

        // Clamp prediction agar tidak keluar arena.
        predictedX = Math.Max(WALL_SAFE_MARGIN, Math.Min(ArenaWidth - WALL_SAFE_MARGIN, predictedX));
        predictedY = Math.Max(WALL_SAFE_MARGIN, Math.Min(ArenaHeight - WALL_SAFE_MARGIN, predictedY));

        // Gambar posisi prediksi musuh.
        var g = Graphics;
        Pen redPen = new Pen(Brushes.Red);
        g.DrawRectangle(redPen, (float)predictedX, (float)predictedY, 20, 20);

        // Arahkan gun ke posisi prediksi.
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        SetTurnGunLeft(bearingFromGun);
    }

    // Dipanggil saat bot menembakkan peluru.
    // Digunakan untuk membuat virtual bullet targeting.
    public override void OnBulletFired(BulletFiredEvent e){
        AddMyVirtualBullet(X, Y, e.Bullet.Speed, e.Bullet.Power, GunDirection * Math.PI / 180, currentTargetId, 0);
        EnemyData data = enemyData[currentTargetId];

        AddMyVirtualBullet(X, Y, e.Bullet.Speed, e.Bullet.Power, DirectionTo(data.LastX, data.LastY) * Math.PI / 180, currentTargetId, 1);
    }

    // Dipanggil saat bot terkena peluru.
    // Digunakan untuk menghitung efektivitas stop-and-go.
    public override void OnHitByBullet(HitByBulletEvent e){
        // Counter hanya aktif saat 1v1.
        if (EnemyCount == 1){
            stopAndGoHitCount++;
        }
    }

    // Dipanggil saat musuh mati.
    // Digunakan untuk reset target.
    public override void OnBotDeath(BotDeathEvent e){
        enemyData[e.VictimId].IsAlive = false;

        // Reset target jika target mati.
        if (e.VictimId == currentTargetId){
            currentTargetDistance = double.PositiveInfinity;
        }
    }

    // Menghitung danger level suatu titik.
    // Dipakai pada anti-gravity movement.
    private double CalcGrav(double candidateX, double candidateY){
        double grav = 0;
        foreach (EnemyData enemy in enemyData.Values){
            if (enemy.IsAlive){
                // Musuh memberi gaya tolak.
                grav += ENEMY_REPULSION_FORCE * (enemy.LastEnergy - ENEMY_ENERGY_THRESHOLD) /
                        (distanceSq(candidateX, candidateY, enemy.LastX, enemy.LastY) + EPSILON);
            }
        }

        foreach (Bullet bullet in bullets){
            // Membuat garis arah bullet.
            Line2D bulletLine = new Line2D(
                bullet.X - Math.Cos(bullet.Direction) * 10000,
                bullet.Y - Math.Sin(bullet.Direction) * 10000,
                bullet.X + Math.Cos(bullet.Direction) * 10000,
                bullet.Y + Math.Sin(bullet.Direction) * 10000
            );

            // Jarak titik terhadap lintasan bullet.
            double d = bulletLine.DistanceToPoint(candidateX, candidateY);

            // Semakin dekat ke bullet,
            // gravity makin besar.
            grav += BULLET_REPULSION_FORCE * bullet.Power / (d * d + EPSILON);

        }

        // Mencegah bot diam di tempat yang sama.
        grav += LAST_POSITION_REPULSION * rand.NextDouble() /
                (Math.Pow(DistanceTo(candidateX, candidateY), 2) + EPSILON);


        // Menjaga jarak terhadap target.
        if (currentTargetId != 0){
            double desiredDistance = EnemyCount == 1
                ? AGGRESSIVE_DISTANCE_TARGET
                : currentTargetDistance;
            grav += desiredDistance - DistanceTo(
                enemyData[currentTargetId].LastX,
                enemyData[currentTargetId].LastY);
        }

        // Sudut arena diberi gravity tambahan.
        grav += CORNER_REPULSION_FORCE / distanceSq(candidateX, candidateY, 0, 0);
        grav += CORNER_REPULSION_FORCE / distanceSq(candidateX, candidateY, 0, ArenaHeight);
        grav += CORNER_REPULSION_FORCE / distanceSq(candidateX, candidateY, ArenaWidth, 0);
        grav += CORNER_REPULSION_FORCE / distanceSq(candidateX, candidateY, ArenaWidth, ArenaHeight);

        return grav;
    }

    // Membuat virtual bullet musuh.
    // Digunakan untuk simulasi dodge bullet.
    private void AddVirtualBullet(double x, double y, double speed, double power, double direction){
        Bullet bullet = new Bullet{
            Speed = speed,
            Direction = direction,

            // Spawn sedikit di depan posisi musuh.
            X = x + 2 * speed * Math.Cos(direction),
            Y = y + 2 * speed * Math.Sin(direction),
            Power = power
        };
        bullets.Add(bullet);
    }

    // Membuat virtual bullet dengan linear targeting.
    // Digunakan untuk simulasi prediksi peluru linear.
    private void AddLinearVirtualBullet(double x, double y, double speed, double power){
        // Speed bullet.
        double vb = CalcBulletSpeed(power);

        // Arah bot dalam radian.
        double myDir = Direction * Math.PI / 180;

        // Velocity X dan Y bot.
        double vxt = Speed * Math.Cos(myDir);
        double vyt = Speed * Math.Sin(myDir);

        // Posisi bot saat ini.
        double xt = X;
        double yt = Y;

        // Persamaan quadratic intercept.
        double a = Math.Pow(vxt, 2) + Math.Pow(vyt, 2) - Math.Pow(vb, 2);
        double b = 2 * (vxt * (xt - x) + vyt * (yt - y));
        double c = Math.Pow(xt - x, 2) + Math.Pow(yt - y, 2);

        // Determinan quadratic.
        double d = Math.Pow(b, 2) - 4 * a * c;

        // Waktu intercept bullet.
        double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        double t2 = (-b - Math.Sqrt(d)) / (2 * a);
        double t = Math.Max(t1, t2);

        // Prediksi posisi target.
        double predictedX = xt + vxt * t;
        double predictedY = yt + vyt * t;

        // Arah bullet menuju posisi prediksi.
        double linearDirection = Math.Atan2(predictedY - y, predictedX - x);
        Bullet bulletLinear = new Bullet{
            Speed = speed,
            Direction = linearDirection,

            // Spawn bullet sedikit di depan musuh.
            X = x + 2 * speed * Math.Cos(linearDirection),
            Y = y + 2 * speed * Math.Sin(linearDirection),
            Power = power * 2
        };
        bullets.Add(bulletLinear);
    }

    // Menyimpan virtual bullet milik bot.
    // Digunakan untuk evaluasi targeting.
    private void AddMyVirtualBullet(double x, double y, double speed, double power, double direction, int target, int type){
        MyBullet myBullet = new MyBullet
        (
            x + 2 * speed * Math.Cos(direction),
            y + 2 * speed * Math.Sin(direction),
            speed,
            direction,
            power,
            target,
            type
        );
        myBullets.Add(myBullet);
    }

    // Menghitung jarak kuadrat dua titik.
    // Lebih cepat dari sqrt distance biasa.
    private double distanceSq(double x1, double y1, double x2, double y2){
        return Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2);
    }

    // Menghitung jarak dua titik.
    private double distance(double x1, double y1, double x2, double y2){
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}

// Menyimpan state movement musuh.
public struct State{
    // Angular velocity musuh.
    public int AngularVelocity;

    // Speed musuh.
    public int Speed;

    // Acceleration musuh.
    public int Acceleration;

    public State(double angularVelocity, double speed, double acceleration){
        // Quantize angular velocity.
        AngularVelocity = (int)(angularVelocity * 1024);

        // Bulatkan speed.
        Speed = (int)Math.Round(speed);

        // Threshold acceleration.
        double threshold = 0.1;

        if (acceleration < -threshold)
            Acceleration = -1;
        else if (acceleration > threshold)
            Acceleration = 1;
        else
            Acceleration = 0;
    }

    // Membandingkan isi state.
    public override bool Equals(object obj){
        if (obj is State state){
            return state.AngularVelocity == AngularVelocity &&
                   state.Speed == Speed &&
                   state.Acceleration == Acceleration;
        }
        return false;
    }

    // Hash state untuk dictionary.
    public override int GetHashCode(){
        return AngularVelocity.GetHashCode() ^ Speed.GetHashCode() ^ Acceleration.GetHashCode();
    }
}

// Menyimpan urutan state untuk N-Gram.
public class StateSequence
{
    public List<State> States { get; }
    public StateSequence(IEnumerable<State> states){
        States = new List<State>(states);
    }

    // Membandingkan sequence state.
    public override bool Equals(object obj){
        if (obj is StateSequence seq)
        {
            if (States.Count != seq.States.Count)
                return false;
            for (int i = 0; i < States.Count; i++)
            {
                if (!States[i].Equals(seq.States[i]))
                    return false;
            }
            return true;
        }
        return false;
    }

    // Hash sequence untuk dictionary.
    public override int GetHashCode(){
        int hash = 17;
        foreach (var s in States)
            hash = hash * 31 + s.GetHashCode();
        return hash;
    }
}

// Menyimpan seluruh data musuh.
public class EnemyData{
    // Histori movement musuh.
    public List<State> StateHistory { get; } = new List<State>();

    // Tree prediction N-Gram.
    public Dictionary<StateSequence, TransitionSegmentTree> NgramTree { get; } = new Dictionary<StateSequence, TransitionSegmentTree>();

    // Score targeting.
    public List<int> Type { get; set; } = new List<int> { 5, 0 };

    public bool HasPrevious { get; set; } = false;
    public bool IsAlive { get; set; } = true;

    public double LastDirection { get; set; }
    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastEnergy { get; set; }
    public double LastSpeed { get; set; }
}

// Struktur data peluru.
public struct Bullet{
    public double X;
    public double Y;
    public double Speed;
    public double Direction;
    public double Power;
}

// Menyimpan virtual bullet bot.
public class MyBullet{
    public Bullet BulletData;
    public int Target;
    public int Type;

    public MyBullet(double x, double y, double speed, double direction, double power, int target, int type){
        BulletData = new Bullet { X = x, Y = y, Speed = speed, Direction = direction, Power = power };
        Target = target;
        Type = type;
    }
}

// Membuat garis 2D untuk bullet gravity.
public class Line2D{
    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public Line2D(double x1, double y1, double x2, double y2){
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    // Menghitung jarak titik ke garis.
    public double DistanceToPoint(double px, double py){
        return Math.Abs((Y2 - Y1) * px - (X2 - X1) * py + (X2 * Y1 - Y2 * X1))
                / Math.Sqrt(Math.Pow(Y2 - Y1, 2) + Math.Pow(X2 - X1, 2));
    }
}

// Menyimpan frekuensi state prediction.
public class TransitionSegmentTree{
    private List<KeyValuePair<State, int>> data;
    private int size;
    private (State state, int frequency)[] tree;
    private Dictionary<State, int> stateToIndex;

    public TransitionSegmentTree(){
        data = new List<KeyValuePair<State, int>>();
        stateToIndex = new Dictionary<State, int>();
        size = 0;
        tree = new (State, int)[0];
    }

    // Menambahkan state prediction.
    public void Add(State s){
        // Tambah frequency jika state sudah ada.
        if (stateToIndex.ContainsKey(s)){
            int idx = stateToIndex[s];
            var kvp = data[idx];
            data[idx] = new KeyValuePair<State, int>(s, kvp.Value + 1);
        }
        else {
            // Tambah state baru.
            stateToIndex[s] = data.Count;
            data.Add(new KeyValuePair<State, int>(s, 1));
        }
        RebuildTree();
    }

    // Rebuild segment tree frequency.
    private void RebuildTree(){
        int n = data.Count;
        if (n == 0){
            tree = new (State, int)[0];
            size = 0;
            return;
        }
        size = 1;
        while (size < n) size *= 2;
        tree = new (State, int)[2 * size];

        // Isi leaf node.
        for (int i = 0; i < size; i++){
            if (i < n)
            {
                tree[size + i] = (data[i].Key, data[i].Value);
            }
            else
            {
                tree[size + i] = (default(State), 0);
            }
        }

        // Build parent node.
        for (int i = size - 1; i > 0; i--){
            var left = tree[2 * i];
            var right = tree[2 * i + 1];
            tree[i] = left.frequency >= right.frequency ? left : right;
        }
    }

    // Mengambil state paling sering muncul.
    public State GetMostFrequent(){
        if (tree.Length > 0){
            return tree[1].state;
        }
        return default(State);
    }
}
