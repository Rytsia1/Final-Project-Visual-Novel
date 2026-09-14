# Laporan Simulasi Balancing Gameplay 60 Hari (Headless Simulation Bab 4)

**Tanggal Eksekusi**: 14 September 2026
**Metode Pengujian**: Headless Time-Management Simulation (60 Siklus Hari Kalender)
**Arketipe Pengujian**: Study-Heavy, Social-Heavy, Balanced, Rest-Heavy (Total 240 Snapshot Harian)

---

## 1. Ringkasan Eksekutif Hasil Simulasi (Hari 60)

| Arketipe Perilaku | Status UTS (H30) | Predikat UAS (H60) | Teori / Praktis | Bahasa | Avg Guanxi | Max Lonely | Rumor Peak | Total Burnout |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **StudyHeavy** | `LULUS` | Sangat Memuaskan | 100 / 100 | 100 | 0.0 | 100 | Lv 5 | 56 hari |
| **SocialHeavy** | `LULUS` | Lulus Memuaskan | 100 / 100 | 20 | 68.0 | 20 | Lv 0 | 0 hari |
| **Balanced** | `LULUS` | Lulus Memuaskan | 100 / 100 | 20 | 7.5 | 70 | Lv 0 | 0 hari |
| **RestHeavy** | `LULUS` | Lulus Memuaskan | 100 / 100 | 20 | 0.0 | 100 | Lv 5 | 0 hari |

---

## 2. Analisis Perilaku & Trade-Off Arketipe

### A. Study-Heavy (Akademik Ekstrem)
- **Karakteristik**: Mengabaikan interaksi sosial dan istirahat demi memaksimalkan stat akademik dan kemahiran bahasa Mandarin.
- **Hasil Ujian**: Lulus UTS dan UAS dengan nilai teori dan praktik sempurna (100/100) serta bahasa 100/100.
- **Konsekuensi Patologis**: Vitalitas jatuh ke titik kritis (PH 8, MH 5), memicu **56 hari kondisi Burnout**. Akibat isolasi sosial penuh, Loneliness seluruh NPC menyentuh 100 dan memicu eskalasi rumor hingga level maksimum (Level 5).
- **Kesimpulan Balancing**: Menghukum gaya bermain *grinding* buta secara realistis melalui mekanisme *burnout* dan sanksi reputasi kampus.

### B. Social-Heavy (Sosialisasi Ekstrem)
- **Karakteristik**: Memprioritaskan hangout, makan siang bersama, dan chatting WeTalk untuk menjaga keharmonisan relasi dengan 4 NPC.
- **Hasil Ujian**: Berhasil lulus evaluasi akademik berkat keikutsertaan kelas wajib (Teori 100, Praktik 100), namun kemahiran bahasa tertinggal di angka dasar (20/100).
- **Kondisi Sosial**: Menghasilkan Guanxi tertinggi (Rata-rata 68/100), Loneliness NPC terkendali (< 20), nol ledakan rumor (Level 0), dan **0 hari Burnout**.
- **Kesimpulan Balancing**: Model sosial terbukti sangat efektif mencegah degradasi psikologis Devano, namun memerlukan insentif tambahan untuk studi mandiri bahasa.

### C. Balanced (Pemain Ideal)
- **Karakteristik**: Menjalankan strategi adaptif — belajar mandiri saat vitalitas mencukupi, mengambil istirahat saat lelah, merawat NPC dengan Loneliness >= 50 pada akhir pekan, dan menyapa kawan via WeTalk di malam hari.
- **Hasil Ujian**: Lulus UTS dan UAS secara optimal (Teori 100, Praktik 100).
- **Vitalitas & Sosial**: Menjaga PH (100) dan MH (100) tetap prima tanpa ada hari *Burnout* (0 hari). Mampu meredam puncak loneliness di bawah ambang batas bahaya sehingga rumor tetap berada pada **Level 0 (Aman)**.
- **Kesimpulan Balancing**: Mewakili jalur kemenangan (*golden path*) yang stabil dan sehat bagi mahasiswa internasional.

### D. Rest-Heavy (Pasif / Konservatif)
- **Karakteristik**: Memilih tidur dan istirahat asrama di hampir seluruh blok waktu bebas.
- **Hasil**: Kesehatan fisik dan mental selalu maksimal (100/100, 0 hari Burnout), dan lulus kelas wajib.
- **Kelemahan**: Kemahiran bahasa dan etika budaya tidak berkembang (20/100 dan 15/100), serta seluruh relasi sosial mati (*decay* total, Guanxi 0, Loneliness 100, Rumor Level 5).

---

## 3. Dataset Telemetri Harian (Sampel Cuplikan 10 Hari Pertama)

```csv
﻿Archetype,Day,DayName,PH,MH,Language,Etiquette,Theory,Practice,AvgGuanxi,MaxLoneliness,RumorLevel,IsBurnedOut
StudyHeavy,1,Senin,80,60,28,15,33,51,18.00,10,0,0
StudyHeavy,2,Selasa,60,40,36,15,36,62,16.00,20,0,0
StudyHeavy,3,Rabu,40,20,44,15,39,73,14.00,30,0,0
StudyHeavy,4,Kamis,20,10,52,15,42,84,12.00,40,0,0
StudyHeavy,5,Jumat,8,5,56,15,45,95,10.00,50,0,1
StudyHeavy,6,Sabtu,8,5,60,15,49,99,8.00,60,0,1
StudyHeavy,7,Minggu,8,5,64,15,53,100,6.00,70,0,1
StudyHeavy,8,Senin,8,5,68,15,55,100,4.00,80,1,1
StudyHeavy,9,Selasa,8,5,72,15,57,100,2.00,90,2,1
StudyHeavy,10,Rabu,8,5,76,15,59,100,0.00,100,3,1
StudyHeavy,11,Kamis,8,5,80,15,61,100,0.00,100,4,1
StudyHeavy,12,Jumat,8,5,84,15,63,100,0.00,100,5,1
StudyHeavy,13,Sabtu,8,5,88,15,67,100,0.00,100,5,1
StudyHeavy,14,Minggu,8,5,92,15,71,100,0.00,100,5,1
```


## 4. Lokasi File Dataset untuk Pengolahan Lanjutan (SPSS / Excel / Python)
1. Master Dataset: `Analytics/telemetry_dataset_all_archetypes.csv` (240 baris)
2. Study-Heavy Dataset: `Analytics/telemetry_dataset_studyheavy.csv` (60 baris)
3. Social-Heavy Dataset: `Analytics/telemetry_dataset_socialheavy.csv` (60 baris)
4. Balanced Dataset: `Analytics/telemetry_dataset_balanced.csv` (60 baris)
5. Rest-Heavy Dataset: `Analytics/telemetry_dataset_restheavy.csv` (60 baris)
6. Salinan Dokumen Sistem: `Documents/Analytics/` dan `%USERPROFILE%/Documents/Tokimeki_Analytics/`