# PANDUAN PENGUJIAN & MENJALANKAN GAME "PULAU MONSTER"

Dokumen ini berisi panduan lengkap untuk membuka proyek di Unity Editor 6, menguji alur gameplay, serta menjalankan standalone Windows executable yang telah selesai di-build.

---

## 1. Menjalankan Standalone Executable (Langsung Main)

Game telah berhasil dikompilasi secara penuh menjadi executable Windows mandiri tanpa memerlukan instalasi tambahan:

1. Buka folder:
   ```
   D:\New folder\Pulau-Monster\Builds\PulauMonster_Win\
   ```
2. Jalankan berkas:
   ```
   PulauMonster.exe
   ```
3. Game akan langsung berjalan pada resolusi 1280×720 (skala piksel 320×180 murni, aspect ratio 16:9).

---

## 2. Kontrol Permainan (Keyboard Controls)

| Aksi | Tombol Keyboard | Deskripsi |
|---|---|---|
| **Gerak Karakter** | `W`, `A`, `S`, `D` atau Tombol Panah (`↑`, `←`, `↓`, `→`) | Bergerak 4 arah (kecepatan 4.0 u/s) |
| **Lari Cepat (Sprint)** | Tahan `Left Shift` atau `Right Shift` | Meningkatkan kecepatan menjadi 6.0 u/s |
| **Interaksi / Konfirmasi** | `E`, `Space`, atau `Enter` | Bicara dengan NPC, periksa objek, pilih menu dialog/battle |
| **Batal / Kembali** | `Esc` atau `Backspace` | Kembali dari submenu |
| **Menu Jeda (Pause)** | `Esc` | Buka menu jeda di dunia overworld (Lanjut, Simpan, Menu Utama) |
| **Mute Suara** | `M` | Matikan / nyalakan audio (BGM & SFX) secara instan |

---

## 3. Membuka Proyek di Unity Editor

1. Buka **Unity Hub**.
2. Klik tombol **Add** (atau **Open**) -> pilih folder:
   ```
   D:\New folder\Pulau-Monster
   ```
3. Pastikan versi editor yang digunakan adalah **Unity 6000.3.24f1** (URP 2D).
4. Tunggu Unity selesai mengimpor aset dan mengompilasi skrip (0 error kompilasi).

---

## 4. Menu Otomasi Editor (Idempotent Generator Pipeline)

Proyek ini dilengkapi alat otomatisasi penuh di bilah menu atas Unity (**Pulau Monster**):

- **Pulau Monster > 00 Run All**: Menjalankan seluruh pipeline 01 sampai 10 secara berurutan dalam satu klik.
- **Pulau Monster > 01 Apply Project Settings**: Konfigurasi Tags, Layers (`Player`, `Interactable`, `Wild`, `CameraBounds`), Sorting Layers (`Ground`, `Decor`, `Characters`), dan Custom Transparency Sort Axis `(0, 1, 0)`.
- **Pulau Monster > 02 Setup Pixel Art Pipeline**: Impor aset, konfigurasi PPU 16, Point Filter, No Compression, dan pengirisan spritesheet otomatis (karakter, monster, tileset, UI, VFX).
- **Pulau Monster > 03 Build Sandbox Scene**: Membuat adegan `Scn_Sandbox.unity` untuk pengujian fisika dan collider.
- **Pulau Monster > 04 Build Player Assets and Rule Tiles**: Membuat `Cfg_Player.asset` dan animasi gerak 4 arah.
- **Pulau Monster > 05 Build Monster Data and Overrides**: Membuat 12 ScriptableObject monster lengkap dengan formula status dan kurva level.
- **Pulau Monster > 06 Build World Scene**: Membuat peta overworld 40×30 tile (Desa, Hutan, Gua), penempatan NPC, monster liar, dan Cinemachine 3 Confiner.
- **Pulau Monster > 07 Build Battle Scene**: Membuat arena pertarungan turn-based lengkap dengan panel HUD pemain dan musuh.
- **Pulau Monster > 08 Build Title and End Scenes**: Membuat `Scn_Title.unity` dan `Scn_End.unity`.
- **Pulau Monster > 09 Build Bootstrap Scene and Lighting**: Mengonfigurasi URP 2D Global Light, Point Lights di Gua Petir, dan `Scn_Bootstrap.unity`.
- **Pulau Monster > 10 Apply Build Settings**: Menyusun daftar 5 adegan aktif ke dalam Editor Build Settings.
- **Pulau Monster > Z Build Windows Executable**: Mengompilasi build standalone ke `Builds/PulauMonster_Win/PulauMonster.exe`.

---

## 5. Menguji Alur Gameplay di Unity Editor

### Fitur Play From Bootstrap Otomatis
Skrip `PlayFromBootstrap.cs` secara otomatis mengarahkan tombol **Play** di Unity Editor ke adegan `Scn_Bootstrap` terlebih dahulu untuk memastikan seluruh layanan inti (`AudioService`, `SaveService`, `SceneFlow`, `PlayerParty`) terinisialisasi secara sempurna sebelum memuat adegan tujuan.

### Skenario Pengujian Lengkap:
1. **Layar Judul (`Scn_Title`):**
   - Tekan tombol **Play** di Unity Editor.
   - Layar judul menampilkan panorama pulau beresolusi tinggi, alunan `BGM_Title`, dan judul lengkap **"PULAU MONSTER"** yang terpampang jelas dan megah di bagian atas tengah (lengkap dengan outline emas dan lencana petualangan).
   - Pilih menu **"Mulai Baru"** lalu tekan `Space` / `Enter`.

2. **Desa Awal (`Scn_World`):**
   - Karakter pemain muncul di Desa Awal bersama alunan `BGM_Village`.
   - Dekati **Tetua Desa** (`NPC_Elder`) dan tekan `E` atau `Space`.
   - Tetua akan meminta kamu memilih salah satu dari 3 Monster Starter:
     - **Pyrit** (Elemen Pyro / Api)
     - **Blubi** (Elemen Aqua / Air)
     - **Sprig** (Elemen Sprout / Tumbuhan)
   - Pilih monster pilihanmu dan konfirmasi dialog "Yakin? -> Ya".
   - Setelah mendapatkan monster, monster partnermu akan muncul di sebelah HUD kiri atas dengan potret, nama, `Lv. 03`, dan angka `HP 54 / 54`.
   - Kamu dapat memulihkan HP partnermu kapan saja dengan berinteraksi ke **Batu Simpan** di dekat Tetua.

3. **Membuka Batas Hutan:**
   - Berjalanlah ke arah utara menuju gerbang pembatas.
   - **Penjaga Batas** (`NPC_Guard`) akan memeriksa statusmu. Karena kamu telah memiliki partner monster, Penjaga akan membukakan jalan menuju Hutan Lumut.
   - Dekati pembatas pohon di `(16, 16)` untuk menguji collider (pemain berhenti mulus di `y ≈ 15.59`).

4. **Hutan Lumut & Pertarungan Liar (`Scn_Battle`):**
   - Memasuki wilayah Hutan Lumut ditandai dengan notifikasi Toast dan transisi musik ke `BGM_Forest`.
   - Rumput bergoyang saat dilewati (`GrassRustle`).
   - Monster liar berkeliaran menggunakan AI State Machine (`WanderState`, `ChaseState` saat mendekat).
   - Bersentuhan dengan monster liar memicu efek transisi layar (Screen Fade) ke adegan pertarungan `Scn_Battle`.
   - **Panel Informasi Monster Lengkap:**
     - **Panel Lawan (Kiri Atas):** Menampilkan nama monster lawan, level (misal: `Lv. 03`), elemen (`VOLT`), bar HP teranimasi, serta nilai angka HP (`HP 60 / 80`).
     - **Panel Partner (Kanan Bawah):** Menampilkan nama monster partnermu, level (misal: `Lv. 05`), elemen (`PYRO`), angka HP langsung (`HP 80 / 100`), bar HP teranimasi dinamis, dan bar EXP.
   - Alur Pertarungan Turn-Based:
     - **Serang:** Serangan fisik biasa dengan efek tebasan energi sabit `VFX_Slash`.
     - **Jurus:** Serangan elemen berdaya rusak 1.5x (kuota 3x penggunaan). Menghasilkan partikel pixel burst sesuai warna elemen dan guncangan layar (Camera Shake).
     - **Ramuan:** Memulihkan 30 HP monster partnermu (angka HP bertambah secara real-time).
     - **Bertahan:** Mengurangi damage yang diterima pada giliran berikutnya sebesar 50%.
     - **Lari:** Peluang kabur kembali ke dunia overworld.
   - Menang pertarungan memutar jingle kemenangan, memberikan EXP, dan meningkatkan level monster.
   - Saat mencapai Level 5 dan Level 9, animasi evolusi dengan cincin emas (`UI_Ring`) dan kilatan cahaya putih akan terpicu secara otomatis!

5. **Gua Petir & Pertarungan Boss:**
   - Lanjutkan perjalanan ke arah timur laut menuju Gua Petir.
   - Saat melintasi zona gua, sistem **URP 2D LightZone** secara dinamis meredupkan cahaya global menjadi 0.15, menyalakan obor Point Light pemain (radius 3.5), dan memancarkan pendaran dari 5 kristal ambient.
   - Musik berganti menjadi `BGM_Cave`.
   - Di ujung terdalam gua, kamu akan menemukan Boss **Raijugar** (Monster Volt Stage 3).
   - Menghadapi Boss memicu alunan musik epik `BGM_Boss`.

6. **Layar Tamat (`Scn_End`):**
   - Mengalahkan Boss memicu transisi ke adegan penutup `Scn_End`.
   - Menampilkan teks "DEMO SELESAI", ilustrasi monster partner terakhirmu, statistik waktu bermain (format `mm:ss`), serta ucapan terima kasih.
   - Tekan `Enter` atau klik layar untuk kembali ke Layar Judul.

7. **Uji Coba Sandbox (`Scn_Sandbox`):**
   - Buka `Assets/_Project/Scenes/Sandbox/Scn_Sandbox.unity`.
   - Tekan tombol **Play** (bisa dinonaktifkan sementara mode bootstrap jika ingin menguji fisika murni via `PlayFromBootstrap`).
   - Berjalanlah lurus ke arah kanan menabrak dinding uji `Wall_Test` di `(2.5, 0)`.
   - Verifikasi karakter berhenti tepat di koordinat `x ≈ 1.69` tanpa menembus dinding.
