# 🤖 Tank Royale — Penerapan Algoritma Greedy
**Tugas Besar IF25-21013: Strategi Algoritma**  
Institut Teknologi Sumatera — 2026

---

## 📌 Deskripsi Singkat Algoritma Greedy

Proyek ini mengimplementasikan **algoritma greedy** pada empat bot Tank Royale yang saling berkompetisi di arena Robocode. Prinsip utama algoritma greedy yang diterapkan adalah **pengambilan keputusan terbaik secara lokal pada setiap turn**, tanpa melakukan pencarian menyeluruh terhadap seluruh kemungkinan state di masa depan.

Keempat bot yang dikembangkan masing-masing menerapkan heuristik greedy yang berbeda:

| Bot | Pendekatan Greedy |
|---|---|
| **Kamikaze** | Greedy Agresif — selalu menerjang dan menembak musuh terdekat dengan daya maksimal |
| **RotasiBumi** | Greedy Rule-Based — memilih aksi gerak dan tembak berdasarkan evaluasi jarak spasial |
| **Sweepy** | Greedy Wall-Alignment — merapat ke dinding terdekat untuk meminimalisasi sudut serang |
| **RyoikiTenkai** | Anti-Gravity + N-Gram Prediction — navigasi posisi teraman & prediksi titik tembak masa depan |

Bot utama yang digunakan dalam kompetisi adalah **RyoikiTenkai**, sebagai strategi paling adaptif dan seimbang.

---

## ⚙️ Requirements

- **Java JDK** versi 11 atau lebih baru
- **Robocode Tank Royale** (versi asisten melalui starter guide tubes)
- Sistem operasi: Windows / macOS / Linux

---

## 🛠️ Instalasi

1. Pastikan **Java JDK** sudah terinstal. Cek versi dengan:
   ```bash
   java -version
   ```

2. Clone repository ini:
   ```bash
   git clone https://github.com/ChiruNukai/TubesStima2026-Kelompok_MBG_MieayamBiniGue
   cd TubesStima2026-Kelompok_MBG_MieayamBiniGue
   ```

## 👥 Author

| Nama | NIM |
|---|---|
| Albert Christian Sihaloho | 124140178 |
| Jalaludin Mufadhol Al Faruq | 124140154 |
| Jundi Lamtara | 124140190 |

**Dosen Pengampu:** Imam Eko Wicaksono, S.Si, M.Si.  
**Program Studi:** Teknik Informatika — Institut Teknologi Sumatera