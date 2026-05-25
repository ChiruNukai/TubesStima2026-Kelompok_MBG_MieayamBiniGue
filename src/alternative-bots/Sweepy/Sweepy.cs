using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
 
// ------------------------------------------------------------------
// 🧹 Sweepy
// ------------------------------------------------------------------
// Pergerakan : Wall Hugging (menyusuri tembok arena)
// Penargetan : Reactive Targeting (tembak saat scan / tabrakan)
// ------------------------------------------------------------------
/*
                                                  **                                                
                                              +==+***++                                             
                                         ++++***********+*                                          
                                        ******#************#                                        
                                       ####%#%#**************                                       
                                     *#%%%%%%#****************                                      
                                  %%%%%%%%% #******#*********** ****                                
                                 ########   +=+*-=***+*******#*****                                 
                               ++*########****#+==+*#***+=---***++*                                 
                             ++++############****++**+=====+#**==*                                  
                             **+  %##############***#####%%##+==*                                   
                          +=##      %#########***##********###**###                                 
                      *++++=+          ######*++*+:.+##*******#**#*#                                
                      ++--            #***##**+++:..-=++********+*#**#                              
                                     ##*##***+++#*=-:...:--+##******+**#                            
                                       %#***+++==+-:...:##**+*#####****##      ::-                  
                                       #****+++-:::....:::-++**##      -:-   ::-                    
                                      *****#+++-........:-+++##         -:.::::-:-                  
                                     ******%*++*-.......:+++***          =:.:-::-+                  
                                   #+*+***  *++#+-:::-*##*++****         =:....:+                   
                              ****++=+*****#**+*##++%*+##*+#****#      **--:.:-                     
                        #******++==+********#*+*#########+******##    #++-:=-=+                     
                    #**#**:++====+***#+********+*##***##*+:=*******# ##+-:-=**##                    
               *****  -:-==::==+***  *******#***#***#***##****#****#####=-+###*#                    
            *++*#   ====--:..:+*    *******###*********#=***=*#***######*+*%###*#                   
          *++*   +===++-:--:..:=  %*********##*******++=++**-*###########*+#%##**                   
         ++*   *++++=+++:......-=*#********###***********++***###*####*---*+%###*#                  
        ++*  #**++++*+*   ==:::-:=*#########%#************+***#####***+**==+%###*++                 
       *+*  ****+**       #*++=---*******##% #*************+*##%####*==###*=%##***++                
       +** *****+#       #*##%#=-=*****####% ##**************#%  ########*-=###**+**+               
       +****+**+#        *###%%%#*******##%   ##**###*******+##%  %######+-+##***+***+              
       ****+****        #*###%%%+******##%   #########********##    %####=+#*******+**              
        ***+****        **###%%+=*#==###%    ########*******#*#%       *=+**+******+**              
        ***+****       #*###%%#*=#**###%    ########*#******#*###      +*** *********               
         *******       **###%#*#%#####     ###%%%#%%*###*######%#           ********#               
           ******      **###*#######      %#%###%%%%+#*+*++*######           *******                
             *****     *###*++*##        ###%%%%%%##%%**##%*####%%%          *****                  
                ++++   **#*+++         %###%%%%%%%*****++******#%%%%       ==***                    
                *++*   ***==+        %@#####%%%%%*++++**#+--====*%###     ++++=                     
                 *-=   #*+=+        #%%##########++*######+:.:.:-*%%##   +--                        
                 ###    *++*+++++++++#%%###%#**######*+-::...:..-=*%%#% #**                         
                 *## ***++==++++++***#######%*##=-:-:....:...::..-=####  +=                         
              ==++++++====++++++***###########*=..:-....::....:..:-+#### -=+                        
              ++++++=+++++++++*##*###########*=..:--....::...:-:.:-=*### #++=                       
              +++++++*++++***##**############=-:-=+=...:-=:.:-+-:---+*##%                           
             +++++**+++***##%***###########*--:::-----::---:::--::+##+###                           
             ++++++++***##  #**###########*=#*-:::::::::=-::::::*#%%%**###                          
            *+++++++***#   #+*###########*=###=:........--::...-######+###                          
            *+++++++***   *++###########*=*###*:...::--==-:....=######*+###                         
            *++=+++***   *++*###########=*#############%%%*=***########++##                         
             *++++***   *++*###########=+******%%%#####%#%%#%%#%########+*##                        
                ****  ++++*###########++*******#%#*++++=#%%#%%#%#########+*##                       
                    %+--=+*##########*+*********%%%%%%%%##%#%#*##*#######*+###                      
                  *=*+=.=+###########*+*#      %%%%##########%#****#######*=###                     
                *==*:-+=:+*#########*++         ##############% %***########=***                    
             #####*+---===+*########*+*       #####%##%%%#%%%%#     #***####*++=**                  
            ######*+==--++--*-+**=+**+       ########%%%%%%%%%%      *********+=+*+=                
           +#*+##*++++=:.-+*=:==::-*++      %########%%###%%%%%       #*******++**+-=               
          %+#+=-=++-=-....-*****++*++       ########% #######%%         #******+*--++##             
           ++--++-=+++:....+-+*+-=*++      ########%  ######%%              ##**+*+**##+*           
           ==+=--=+++-.....+-+=::=*--     %#######%   ######%%                  **#*==*#-+*         
           -++++-++=:..-*=-********-=     ######%%    #####%%                   **###***=+==        
          -=++=-=:....=***#**++****-=    ######%      ####%%                     +*#++=-=+*#=       
          -=++++=:...:+*****=+*+++*=-   %#####%       ####%%                     **##%*+#**#*=      
          --++++=-....:*****=====+*+-   #####%        ###%%                       +*##*+####+-      
           -=++=:-=-:++=--====******-+ %####%        %###%%                       *+#%%%###*-*      
            =-+=:-++=*=-+#***+=+#**#-=%%##%%         %%##%                         +*#**##+=        
              =-=++++*********#*++=+=+%###%          %%##%                         #+++++++         
                 =:-++****####*+===--=###%%         %#%##%                          +++             
                    +=--=====---===#####%%          ####%%                                          
                                  +*#####%          #####%                                          
                                    %####%%         ######%                                         
                                    ######%         %#####%                                         
                                    ++####*         %######                                         
                                    *+=++===        ==-===-*                                        
                                     *++=--=        ++****++                                        
                                         *              #                                           

*/
// ------------------------------------------------------------------

public class Sweepy : Bot{
    // Entry point program. Membuat instance Sweepy dan memulai bot.
    static void Main(string[] args) => new Sweepy().Start();
 
    // Memuat konfigurasi bot dari file Sweepy.json.
    Sweepy() : base(BotInfo.FromFile("Sweepy.json")) { }
 
    // Menyimpan data musuh terakhir yang terdeteksi oleh radar.
    // Digunakan untuk mengarahkan tembakan dan mendeteksi kematian musuh.
    private ScannedBotEvent sc = null;
 
    // Method utama yang dijalankan saat ronde dimulai.
    public override void Run(){
        // Warna bot.
        BodyColor = Color.Purple;
        TurretColor = Color.Black;
        RadarColor = Color.Magenta;
        BulletColor = Color.Black;
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);
 
        // Gun tidak ikut rotasi body.
        // Radar tidak ikut rotasi gun.
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
 
        // Putar badan ke arah tembok terdekat sebelum mulai bergerak.
        TurnLeft(CalcAngleToNearestWall());
 
        // Loop utama: maju terus sambil radar berputar penuh
        // untuk mendeteksi musuh di semua sudut arena.
        while (IsRunning){
            SetForward(1000);
            SetTurnRadarLeft(360);
            Go();
        }
    }
 
    // Menghitung sudut putar badan menuju tembok terdekat.
    private double CalcAngleToNearestWall(){
        // Hitung jarak bot ke setiap sisi arena.
        double distLeft   = X;
        double distRight  = ArenaWidth - X;
        double distBottom = Y;
        double distTop    = ArenaHeight - Y;
 
        // Ambil jarak minimum dari keempat sisi.
        double minDist    = Math.Min(Math.Min(distLeft, distRight), Math.Min(distBottom, distTop));
 
        // Tentukan heading target berdasarkan tembok terdekat.
        // 270 = kiri, 90 = kanan, 180 = bawah, 0 = atas.
        double targetHeading;
        if      (minDist == distLeft)   targetHeading = 270;
        else if (minDist == distRight)  targetHeading = 90;
        else if (minDist == distBottom) targetHeading = 180;
        else                            targetHeading = 0;
 
        // Hitung selisih heading (bearing) dan normalisasi ke [-180, 180].
        double bearing = targetHeading - Direction;
        if (bearing > 180)  bearing -= 360;
        if (bearing < -180) bearing += 360;
        return bearing;
    }
 
    // Dipanggil saat radar mendeteksi bot musuh.
    public override void OnScannedBot(ScannedBotEvent e){
        // Simpan data musuh terakhir yang terdeteksi.
        sc = e;
 
        // Tentukan firepower berdasarkan jarak musuh.
        // Semakin dekat musuh, semakin besar power tembakan.
        double dist        = DistanceTo(e.X, e.Y);
        double bulletPower = dist < 150 ? 3 : dist < 300 ? 2 : 1;
 
        // Arahkan gun dan radar ke posisi musuh.
        SetTurnGunLeft(GunBearingTo(e.X, e.Y));
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));
 
        // Tembak hanya jika gun sudah dingin (tidak overheat).
        if (GunHeat == 0)
            Fire(bulletPower);
    }
 
    // Dipanggil saat bot mati karena bot ini.
    public override void OnBotDeath(BotDeathEvent e){
        if (sc != null && e.VictimId == sc.ScannedBotId)
            sc = null;
    }
 
    // Dipanggil saat bot bertabrakan langsung dengan bot lain.
    public override void OnHitBot(HitBotEvent e){
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));
        if (GunHeat == 0)
            Fire(3);
    }
 
    // Dipanggil saat bot menabrak dinding arena.
    public override void OnHitWall(HitWallEvent e){
        TurnLeft(90);
    }
}