# Laporan Pengujian Black-Box: Boundary Value Analysis (BVA)
## Evaluasi Kondisi Event Berbasis Basis Data Relasional SQLite (`EventManager.cs`)

- **Tanggal Pengujian**: 2026-09-14 13:12:36
- **Metodologi**: Black-Box Testing (Boundary Value Analysis - $N-1, N, N+1$)
- **Total Kasus Uji**: 44
- **Tingkat Keberhasilan**: 100% PASS (44/44)
- **Target Pengujian**: `EventManager.Instance.EvaluateConditions(GameEvent e)`

### Tabel Hasil Pengujian Nilai Batas Kritis

| No | Parameter | Kasus Uji (BVA) | Nilai Input | Nilai Batas | Ekspektasi | Hasil Aktual | Kesimpulan |
|:--:|:----------|:----------------|:-----------:|:-----------:|:----------:|:------------:|:----------:|
| 1 | Language | Bawah Batas (N-1) | 39 | >= 40 | FAIL | FAIL | **PASS** |
| 2 | Language | Titik Batas (N) | 40 | >= 40 | PASS | PASS | **PASS** |
| 3 | Language | Atas Batas (N+1) | 41 | >= 40 | PASS | PASS | **PASS** |
| 4 | Etiquette | Bawah Batas (N-1) | 29 | >= 30 | FAIL | FAIL | **PASS** |
| 5 | Etiquette | Titik Batas (N) | 30 | >= 30 | PASS | PASS | **PASS** |
| 6 | Etiquette | Atas Batas (N+1) | 31 | >= 30 | PASS | PASS | **PASS** |
| 7 | Physical Health | Bawah Batas (N-1) | 29 | >= 30 | FAIL | FAIL | **PASS** |
| 8 | Physical Health | Titik Batas (N) | 30 | >= 30 | PASS | PASS | **PASS** |
| 9 | Physical Health | Atas Batas (N+1) | 31 | >= 30 | PASS | PASS | **PASS** |
| 10 | Mental Health (Min) | Bawah Batas (Min-1) | 19 | >= 20 | FAIL | FAIL | **PASS** |
| 11 | Mental Health (Min) | Titik Batas (Min) | 20 | >= 20 | PASS | PASS | **PASS** |
| 12 | Mental Health (Min) | Atas Batas (Min+1) | 21 | >= 20 | PASS | PASS | **PASS** |
| 13 | Mental Health (Max) | Bawah Batas (Max-1) | 79 | <= 80 | PASS | PASS | **PASS** |
| 14 | Mental Health (Max) | Titik Batas (Max) | 80 | <= 80 | PASS | PASS | **PASS** |
| 15 | Mental Health (Max) | Atas Batas (Max+1) | 81 | <= 80 | FAIL | FAIL | **PASS** |
| 16 | Academic Theory | Bawah Batas (N-1) | 49 | >= 50 | FAIL | FAIL | **PASS** |
| 17 | Academic Theory | Titik Batas (N) | 50 | >= 50 | PASS | PASS | **PASS** |
| 18 | Academic Theory | Atas Batas (N+1) | 51 | >= 50 | PASS | PASS | **PASS** |
| 19 | Academic Practice | Bawah Batas (N-1) | 49 | >= 50 | FAIL | FAIL | **PASS** |
| 20 | Academic Practice | Titik Batas (N) | 50 | >= 50 | PASS | PASS | **PASS** |
| 21 | Academic Practice | Atas Batas (N+1) | 51 | >= 50 | PASS | PASS | **PASS** |
| 22 | Guanxi NPC (Haoran) | Bawah Batas (N-1) | 59 | >= 60 | FAIL | FAIL | **PASS** |
| 23 | Guanxi NPC (Haoran) | Titik Batas (N) | 60 | >= 60 | PASS | PASS | **PASS** |
| 24 | Guanxi NPC (Haoran) | Atas Batas (N+1) | 61 | >= 60 | PASS | PASS | **PASS** |
| 25 | Affection State | Bawah Batas (N-1) | State 1 (Neutral) | >= State 2 | FAIL | FAIL | **PASS** |
| 26 | Affection State | Titik Batas (N) | State 2 (Friend) | >= State 2 | PASS | PASS | **PASS** |
| 27 | Affection State | Atas Batas (N+1) | State 3 (Tokimeki) | >= State 2 | PASS | PASS | **PASS** |
| 28 | Global Rumor | Bawah Batas (N-1) | Level 0 (Aman) | >= Level 1 | FAIL | FAIL | **PASS** |
| 29 | Global Rumor | Titik Batas (N) | Level 1 (Bisik) | >= Level 1 | PASS | PASS | **PASS** |
| 30 | Global Rumor | Atas Batas (N+1) | Level 2 (Menyebar) | >= Level 1 | PASS | PASS | **PASS** |
| 31 | Day Range (Min) | Bawah Batas (Min-1) | Hari 09 | >= Hari 10 | FAIL | FAIL | **PASS** |
| 32 | Day Range (Min) | Titik Batas (Min) | Hari 10 | >= Hari 10 | PASS | PASS | **PASS** |
| 33 | Day Range (Min) | Atas Batas (Min+1) | Hari 11 | >= Hari 10 | PASS | PASS | **PASS** |
| 34 | Day Range (Max) | Bawah Batas (Max-1) | Hari 19 | <= Hari 20 | PASS | PASS | **PASS** |
| 35 | Day Range (Max) | Titik Batas (Max) | Hari 20 | <= Hari 20 | PASS | PASS | **PASS** |
| 36 | Day Range (Max) | Atas Batas (Max+1) | Hari 21 | <= Hari 20 | FAIL | FAIL | **PASS** |
| 37 | Time Block | Mismatch (Pagi) | Pagi | Siang | FAIL | FAIL | **PASS** |
| 38 | Time Block | Match (Siang) | Siang | Siang | PASS | PASS | **PASS** |
| 39 | Time Block | Mismatch (Malam) | Malam | Siang | FAIL | FAIL | **PASS** |
| 40 | Day Type | Mismatch (Weekend) | Weekend | Workday | FAIL | FAIL | **PASS** |
| 41 | Day Type | Match (Workday) | Workday | Workday | PASS | PASS | **PASS** |
| 42 | Story Flag | Flag Tidak Terpasang | NULL (0) | >= 1 | FAIL | FAIL | **PASS** |
| 43 | Story Flag | Flag Terpasang (N) | Val 1 | >= 1 | PASS | PASS | **PASS** |
| 44 | Story Flag | Flag Terpasang (N+1) | Val 2 | >= 1 | PASS | PASS | **PASS** |

### Kesimpulan Analisis
Berdasarkan pengujian nilai batas di atas, mesin inferensi kondisi `EventManager.cs` membuktikan akurasi 100% dalam mengevaluasi seluruh batas kritis parameter pemain, kalender, relasi NPC, rumor, dan story flags deklaratif tanpa anomali percabangan logika.
