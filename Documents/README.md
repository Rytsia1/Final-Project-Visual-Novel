# Dokumentasi Artefak Pengujian & Dataset Telemetri (Bab 4 Skripsi)
### *Indeks Laporan Empiris, Hasil Pengujian Black-Box BVA, dan Simulasi Keseimbangan Sistem*

Direktori ini memuat seluruh dokumen evaluasi, dataset telemetri, dan berkas analisis kuantitatif yang menjadi dasar pembahasan **Bab 4 Skripsi / Tugas Akhir** pada proyek permainan *Tokimeki Memorial Style Educational Social Simulation*.

---

## 📑 1. Indeks Dokumen & Laporan Evaluasi

| Berkas | Format | Deskripsi & Ruang Lingkup Analisis | Dokumen Terkait |
| :--- | :---: | :--- | :---: |
| [Laporan_Pengujian_BVA.md](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Laporan_Pengujian_BVA.md) | Markdown | Laporan pengujian fungsional *Black-Box* dengan metode **Boundary Value Analysis (BVA)** terhadap mesin inferensi kondisi `EventManager.cs`. Menguji 44 kasus batas ($N-1, N, N+1$) dengan tingkat kelulusan **100% PASS**. | [Laporan_Pengujian_BVA.csv](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Laporan_Pengujian_BVA.csv) |
| [Laporan_Simulasi_Balancing_60Hari.md](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Laporan_Simulasi_Balancing_60Hari.md) | Markdown | Laporan komparasi mendalam hasil simulasi *headless* selama 60 hari kalender penuh melintasi 4 arketipe pemain (*Study-Heavy*, *Social-Heavy*, *Balanced*, dan *Rest-Heavy*) dengan total 240 snapshot harian. | [Analytics/](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Analytics/) |
| [Laporan_Balancing_Bab4.md](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Laporan_Balancing_Bab4.md) | Markdown | Ringkasan eksekutif evaluasi **6 Metrik Keseimbangan Fundamental** (*Stat Growth Rate*, *Burnout Frequency*, *Rumor Level 2+ Rate*, *Max Loneliness Peak*, *Event Trigger Rate*, *Rata-Rata Vitalitas*). | [Laporan_Balancing_Bab4.csv](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Laporan_Balancing_Bab4.csv) |

---

## 📈 2. Katalog Dataset Telemetri (`Analytics/`)

Dataset mentah hasil eksekusi simulasi *headless* batch tersimpan di subdirektori [Analytics/](file:///d:/Folder%20dio/Porto/My%20project%20%281%29/Documents/Analytics/):

1. **`telemetry_dataset_all_archetypes.csv`**: Agregasi komprehensif seluruh 240 baris data dari ke-4 arketipe pemain dari Hari 1 hingga Hari 60.
2. **`telemetry_dataset_studyheavy.csv`**: Rekaman harian arketipe *Study-Heavy* (menunjukkan degradasi fisik/mental ekstrem dan ledakan bom rumor Level 5).
3. **`telemetry_dataset_socialheavy.csv`**: Rekaman harian arketipe *Social-Heavy* (menunjukkan keharmonisan relasi Guanxi tertinggi dan 0 insiden burnout).
4. **`telemetry_dataset_balanced.csv`**: Rekaman harian arketipe *Balanced* (jalur kemenangan ideal/golden path dengan kesehatan optimal dan kelulusan akademik memuaskan).
5. **`telemetry_dataset_restheavy.csv`**: Rekaman harian arketipe *Rest-Heavy* (strategi pasif konservatif dengan isolasi sosial total).

---

## 📚 3. Kamus Data (*Data Dictionary*) Kolom Telemetri

Setiap baris rekaman pada dataset telemetri memiliki struktur atribut berikut:

| Nama Kolom | Tipe Data | Rentang Nilai | Definisi & Signifikansi Analitis |
| :--- | :---: | :---: | :--- |
| `Archetype` | String | `StudyHeavy`, `SocialHeavy`, `Balanced`, `RestHeavy` | Model profil perilaku kecerdasan buatan / arketipe keputusan bermain yang disimulasikan. |
| `Day` | Integer | 1 s.d. 60 | Nomor hari kalender simulasi dalam satu semester pertukaran akademik. |
| `DayName` | String | `Senin` s.d. `Minggu` | Hari dalam sepekan (menentukan tipe hari *Workday* atau *Weekend*). |
| `PH` | Integer | 0 s.d. 100 | *Physical Health* (kebugaran fisik). Nilai $\le 10$ memicu status *Burnout*. |
| `MH` | Integer | 0 s.d. 100 | *Mental Health* (stabilitas psikologis). Nilai $\le 10$ memicu status *Burnout*. |
| `Language` | Integer | 0 s.d. 100 | Tingkat kemahiran Bahasa Mandarin Devano. |
| `Etiquette` | Integer | 0 s.d. 100 | Skor etika dan kesantunan budaya (*Mianzi* / 面子). |
| `Theory` | Integer | 0 s.d. 100 | Skor pemahaman teori akademik IT (Ambang batas kelulusan UTS: 50). |
| `Practice` | Integer | 0 s.d. 100 | Skor keterampilan praktikum laboratorium IT (Ambang batas kelulusan UTS: 45). |
| `AvgGuanxi` | Float | 0.0 s.d. 100.0 | Rata-rata skor relasi/Guanxi terhadap 4 NPC kampus. |
| `MaxLoneliness` | Integer | 0 s.d. 100 | Tingkat kesepian tertinggi di antara seluruh NPC (Ambang rumor bahaya: $\ge 75$). |
| `RumorLevel` | Integer | 0 s.d. 5 | Tingkat eskalasi rumor kampus global (*Tokimeki rumor bomb mechanic*). |
| `IsBurnedOut` | Integer | 0 atau 1 | Penanda biner apakah karakter berada dalam kondisi kelelahan ekstrem (*Burnout*). |

---

## 🔁 4. Panduan Reproduksi Data Uji (*Reproduction Guide*)

Seluruh data pengujian dalam direktori ini bersifat **deterministik** dan dapat direproduksi kembali kapan saja langsung melalui Unity Editor:

### A. Menjalankan Ulang Pengujian BVA (44 Kasus Uji)
1. Buka proyek pada Unity Editor 6 LTS (`6000.3.16f1`).
2. Pada Menu Bar atas, klik **`Tokimeki TA`** $\rightarrow$ **`Event Condition Test Runner`**.
3. Klik tombol **`Run All 44 BVA Tests`**.
4. Seluruh kasus uji akan dievaluasi dan hasilnya dapat diekspor langsung ke berkas CSV atau format laporan Markdown.

### B. Menjalankan Ulang Simulasi Balancing 60 Hari
1. Pada Menu Bar atas, klik **`Tokimeki TA`** $\rightarrow$ **`Headless Balancing Simulator`**.
2. Pilih mode arketipe yang diinginkan atau pilih opsi *Batch Simulation* (4 arketipe sekaligus).
3. Klik tombol **`Jalankan Simulasi 60 Hari`**.
4. Untuk mengekspor seluruh dataset baru ke folder `Analytics/`, klik **`Tokimeki TA`** $\rightarrow$ **`Export All Archetype Datasets (Batch Headless)`**.
