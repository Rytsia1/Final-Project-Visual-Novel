# Generates Assets/Scripts/Editor/Seeds/day0_prologue.sql from the Day 0 script (day0_kedatangan_vn_v2.md).
# Usage: python Tools/gen_day0_prologue.py Assets/Scripts/Editor/Seeds/day0_prologue.sql
import sys

E = 'Assets/Character/Edelweiss/Edelweiss.png'
NODE0 = 9001
OPT0 = 9001

def sub(han, pinyin, arti):
    return f'{han}\n<size=80%><i>{pinyin} — "{arti}"</i></size>'

# item kinds: ('n', text, sprite) narration | ('s', speaker, text, sprite) | ('c', title, sprite, [(label, flag, items)])
def n(t, sp=''): return ('n', t, sp)
def ed(t): return ('s', 'Edelweiss', t, E)
def dv(t): return ('s', 'Devano', t, '')
def ayi(t): return ('s', 'Āyí', t, '')
def unk(t): return ('s', '???', t, E)
def c(title, opts, sp=E): return ('c', title, sp, opts)

TEGUR_LIRIK = n('Edelweiss menarik ujung lengan jaketku, mengajakku menjauh dari meja sampai ke dekat tangga, cukup jauh sampai suara antrean tinggal dengung.', E)
EDEL_FIX = [
    ed(sub('阿姨，不好意思，可能是我们没说清楚……他的是五零三。', 'Āyí, bù hǎo yìsi, kěnéng shì wǒmen méi shuō qīngchu… tā de shì wǔ líng sān.', 'Bu, maaf, mungkin kami yang kurang jelas ngomongnya… punya dia 503.')),
    ayi(sub('哦哦，好好。', 'Ó ó, hǎo hǎo.', 'Oh, oh, iya iya.')),
    n('Kuncinya ditukar. Āyí bahkan sempat tersenyum.'),
]

SCRIPT = [
    # ADEGAN 1 — GERBANG KAMPUS
    n('Gerbang kampus lebih besar dari yang kubayangkan dari foto-foto di email penerimaan. Dua pilar batu, ukiran aksara yang tidak bisa kubaca, dan satu papan nama panjang yang hanya kukenali huruf romannya di baris paling bawah.'),
    n('Koperku berhenti di celah ubin, roda kirinya nyangkut entah kenapa. Selalu begitu. Kalau ada satu celah di sepuluh meter persegi ubin, roda koperku akan menemukannya.'),
    n('Aku menariknya lagi, lebih keras. Tas kamera di bahu kananku ikut bergoyang, dan aku refleks menahannya dengan siku. Kebiasaan lama. Barang itu lebih penting dari koper. Kalau dipikir-pikir, aneh juga urutan prioritasku.'),
    n('Orang-orang lewat begitu saja. Tidak ada yang menoleh, tidak ada yang perlu menoleh.'),
    n('Di pesawat tadi aku sempat menghafal beberapa kalimat dari aplikasi belajar bahasa. <i>Nǐ hǎo.</i> <i>Xièxie.</i> <i>Bù hǎo yìsi.</i> <i>Bú duì.</i> Ditambah angka satu sampai sepuluh. Aku belum yakin bisa mengucapkan satu pun dengan benar.'),
    n('Aku mengeluarkan kertas konfirmasi asrama dari saku jaket. Gedung C, kamar 503. Kucocokkan dengan papan penunjuk yang setengahnya berbahasa Mandarin, setengahnya bahasa Inggris yang terjemahannya terasa dipaksakan.'),
    n('Belum sempat aku memutuskan arah, ada suara yang memecah semuanya.'),
    ('s', '???', 'DEVANO?!', ''),
    n('Seseorang berlari kecil dari arah pos satpam. Kuncir dua bergelombangnya memantul-mantul, satu tangan melambai terlalu semangat untuk ukuran sore yang sepi begini. Dia berhenti tepat di depanku, agak terengah, lalu buru-buru merapikan napas seolah baru sadar dia barusan lari.', E),
    unk('Kamu Devano kan? Dari Indonesia?'),
    c('Sapaan', [
        ('[Ramah] "Iya, aku Devano. Kamu yang jemput aku, ya?"', 'd0_sapa_ramah', [
            dv('Iya, aku Devano. Kamu yang jemput aku, ya?'),
            ed('Iya! Aku Edelweiss. Dosen bilang aku yang jemput kamu hari ini.'),
            n('Bahunya turun sedikit. Lega, seperti barusan lolos ujian kecil yang cuma dia sendiri yang tahu.', E),
        ]),
        ('[Formal] "Betul. Devano Baskara Pratama, peserta program pertukaran."', 'd0_sapa_formal', [
            dv('Betul. Devano Baskara Pratama, peserta program pertukaran.'),
            n('Dia ikut menegakkan punggung, seolah sedang berhadapan dengan dosen, lalu sadar sendiri dan buru-buru menurunkan bahunya lagi.', E),
            ed('Eh, iya, iya. Aku Edelweiss. Santai aja kali, aku bukan dosen kok.'),
        ]),
        ('[Bercanda] "Iya. Yang kopernya barusan nyangkut di ubin itu."', 'd0_sapa_canda', [
            dv('Iya. Yang kopernya barusan nyangkut di ubin itu.'),
            n('Dia melirik koperku, lalu ubinnya, lalu tertawa. Agak keras, dan sedikit kelamaan.', E),
            ed('Ubin di sini emang jahat. Aku Edelweiss, yang ditugasin jemput kamu.'),
        ]),
    ]),
    n('Dia mengeluarkan sesuatu dari saku pinaforenya, gerakannya refleks, seperti sudah ratusan kali dilakukan. Sebungkus permen kecil.', E),
    ed('Nih. Buat penyambutan.'),
    n('Dia menyodorkannya dengan senyum lebar, lalu cepat-cepat menambahkan,', E),
    ed('Semua orang baru kukasih ini kok, bukan cuma kamu doang.'),
    c('Permen', [
        ('[Tulus] "Makasih. Dari pagi aku belum sempat makan apa-apa, sih."', 'd0_permen_tulus', [
            dv('Makasih. Dari pagi aku belum sempat makan apa-apa, sih.'),
            ed('Hah, serius?! Ih, nanti kutunjukin kantin yang buka sampai malam.'),
            n('Nada suaranya langsung berubah cepat, seperti baru dapat tugas tambahan yang harus segera diurus.', E),
        ]),
        ('[Menggoda] "Oh. Jadi aku bukan spesial, ya."', 'd0_permen_goda', [
            dv('Oh. Jadi aku bukan spesial, ya.'),
            ed('Bukan gitu maksudnya! Maksudku, ya, bukan berarti… aduh.'),
            n('Merahnya merambat sampai ke telinga. Dia menatap permen di tanganku seolah permen itu yang salah.', E),
            ed('Pokoknya terima aja!'),
        ]),
        ('[Singkat] "Makasih."', 'd0_permen_singkat', [
            dv('Makasih.'),
            n('Aku memasukkannya ke saku jaket. Ada jeda kecil sebelum dia menjawab, sepersekian detik saja.', E),
            ed('…Sama-sama!'),
        ]),
    ]),
    n('Dia menepuk kedua tangannya dan berbalik badan.', E),
    ed('Yaudah, ayo! Gedung C di sebelah sana. Agak jauh dikit, tapi santai aja jalan kaki.'),
    n('Aku mengikutinya. Langkahnya lebih cepat dari orang yang sedang jalan santai, seperti ada energi yang perlu disalurkan ke suatu tempat. Dia ramah, tapi ramahnya punya kecepatan sendiri. Seolah kalau dia berhenti sedetik saja, sesuatu yang lain bakal kelihatan.'),

    # ADEGAN 2 — JALAN MENUJU ASRAMA
    n('Jalan menuju asrama melewati deretan pohon yang belum kukenal namanya, lalu kantin yang lampunya baru menyala satu per satu, lalu papan pengumuman penuh kertas warna-warni yang sebagian sudah sobek di ujung.'),
    n('Aku belum sempat memotret apa pun, tapi mataku sudah bekerja sendiri, menyusun komposisi. Siluet gedung asrama dengan lampu koridor yang menyala bergiliran. Lebih menarik daripada gedungnya sendiri.'),
    n('Edelweiss melirik ke arahku. Langkahnya melambat, menyesuaikan langkahku yang masih menyeret koper.', E),
    ed('Kamu excited nggak? Sebulan doang kan di sini?'),
    c('Perasaan', [
        ('[Antusias] "Banget. Aku udah nyiapin daftar tempat yang mau kufoto."', 'd0_excited_antusias', [
            dv('Banget. Aku udah nyiapin daftar tempat yang mau kufoto.'),
            ed('Daftar?! Kamu suka foto? Coba nanti kulihat daftarnya, siapa tahu ada yang salah tempat.'),
            n('Dia mengatakannya dengan nada orang yang sudah lama tinggal di sini, dan kelihatan agak bangga dengan nada itu.', E),
        ]),
        ('[Jujur] "Jujur, lebih ke deg-degan, sih. Belum kebayang apa-apa."', 'd0_excited_jujur', [
            dv('Jujur, lebih ke deg-degan, sih. Belum kebayang apa-apa.'),
            n('Dia diam sebentar. Lalu suaranya turun setengah nada.', E),
            ed('Wajar kok. Aku dulu nangis di minggu pertama.'),
            n('Cepat-cepat, seperti menyesal sudah bilang:', E),
            ed('Tapi cuma sekali! Abis itu udah nggak. Eh, itu tas kamera, ya? Kamu suka foto?'),
        ]),
        ('[Datar] "Lumayan."', 'd0_excited_datar', [
            dv('Lumayan.'),
            ed('Lumayan…'),
            n('Dia mengulang kata itu pelan, seperti sedang menimbang apakah jawabannya perlu dikejar. Lalu matanya jatuh ke tas di bahuku.', E),
            ed('Eh, itu tas kamera? Kamu suka foto?'),
        ]),
    ]),
    ed('Ih, nanti kutunjukin spot-spot bagus deh. Aku udah lama di sini soalnya.'),
    dv('Boleh.'),
    n('Untuk pertama kalinya sejak turun dari bandara, ada sesuatu yang terasa sedikit lebih ringan.'),

    # ADEGAN 3 — LOBI ASRAMA (REGISTRASI)
    n('Lobi Gedung C lebih ramai dari yang kukira. Ada antrean pendek di depan meja kayu dekat pintu masuk: tiga atau empat mahasiswa dengan koper, beberapa sambil main ponsel, satu sambil makan sesuatu dari kantong plastik.'),
    n('Di balik meja duduk seorang ibu paruh baya dengan rompi rajut dan kacamata yang melorot ke ujung hidung. Di sebelahnya ada termos besar, buku catatan tebal, dan papan berisi deretan kunci.'),
    ed('(berbisik) Itu 阿姨, ibu pengelola asrama. Panggil aja <i>Āyí</i>. Semua urusan asrama lewat dia.'),
    n('Antrean maju. Giliranku.'),
    ayi(sub('护照。', 'Hùzhào.', 'Paspor.')),
    n('Aku menyerahkan paspor. Dia membolak-baliknya, menulis sesuatu di buku catatannya dengan cepat, lalu mengambil satu kunci dari papan dan meletakkannya di meja bersama kartu kecil.'),
    ayi(sub('三零五。', 'Sān líng wǔ.', 'Tiga-nol-lima.')),
    n('Aku melihat kartu itu. 305. Lalu kertas konfirmasi di tanganku. 503.'),
    n('Angkanya kebalik.'),
    n('Di belakangku, mahasiswa yang makan dari kantong plastik tadi berhenti mengunyah, menunggu. Aku bisa merasakan antrean itu di punggungku tanpa perlu menoleh.'),
    c('Kesalahan Āyí', [
        ('[Lugas] Ketuk kertas konfirmasi di meja. "不对。五零三。" (Bú duì. Wǔ líng sān.)', 'd0_reg_lugas', [
            n('Aku mengetuk kertas konfirmasi di meja.'),
            dv(sub('不对。五零三。', 'Bú duì. Wǔ líng sān.', 'Salah. Lima-nol-tiga.')),
            n('Suaraku keluar lebih keras dari yang kurencanakan. Aku cuma ingin jelas.'),
            n('Āyí menatap kertasku, lalu melirik sebentar ke antrean di belakangku. Bibirnya merapat jadi garis tipis.'),
            ayi(sub('……知道了。', 'Zhīdào le.', '…Iya, tahu.')),
            n('Dia menukar kuncinya tanpa melihat ke arahku lagi, lalu langsung memanggil orang berikutnya.'),
            TEGUR_LIRIK,
            ed('Dev… soal kunci tadi.'),
            n('Dia ragu sebentar, seperti sedang memilih kata. Tangannya memainkan ujung kuncirnya.', E),
            ed("Kamu nggak salah kok, nomornya emang kebalik. Tapi tadi di belakang kita ada antrean. Kalau kamu bilang 'salah' di depan banyak orang, yang malu itu Āyí-nya. Di sini itu namanya <i>diū miànzi</i>. Kehilangan muka."),
            dv('…Aku cuma mau jelas.'),
            ed('Iya, aku tahu. Makanya caranya aja yang diganti. Bilang <i>bù hǎo yìsi</i> dulu, terus bikin kesannya kayak kamu yang mungkin salah lihat. Nanti dia sendiri yang nemu salahnya. Masalahnya beres, mukanya juga aman.'),
            n('Dia tersenyum kecil, sedikit kikuk.', E),
            ed('Aku sengaja nunggu kita jauh dari meja dulu, lho. Kalau aku ngomongnya di sana, gantian kamu yang malu.'),
            n('Aku teringat bibir Āyí yang merapat tadi. Waktu itu kukira dia cuma sibuk.', E),
        ]),
        ('[Memberi muka] Senyum, tunjukkan kertas pelan-pelan. "阿姨，不好意思……" (Āyí, bù hǎo yìsi…)', 'd0_reg_beri_muka', [
            n('Aku tersenyum, lalu menunjukkan kertas konfirmasi pelan-pelan.'),
            dv(sub('阿姨，不好意思……', 'Āyí, bù hǎo yìsi…', 'Bu, maaf…')),
            dv('…Maybe I read it wrong?'),
            n('Kalimat Mandarinku cuma sampai situ. Sisanya kuserahkan pada kertas yang kutaruh di meja, nomornya menghadap ke dia.'),
            n('Āyí mendorong kacamatanya naik, menyipitkan mata ke kertas, lalu ke buku catatannya sendiri.'),
            ayi(sub('哎呀，我写反了！', 'Āiyā, wǒ xiě fǎn le!', 'Aduh, kebalik nulisnya!')),
            n('Dia tertawa, menukar kuncinya, lalu berkata sesuatu ke Edelweiss sambil menunjukku dengan dagunya.'),
            ayi(sub('这小伙子挺客气的。', 'Zhè xiǎohuǒzi tǐng kèqi de.', 'Anak muda ini sopan juga, ya.')),
            TEGUR_LIRIK,
            ed('Dev. Tadi itu… kamu belajar dari mana?'),
            dv('Aplikasi. Di pesawat. Cuma kalimat itu yang aku hafal.'),
            ed('Serius?! Āyí sampai bilang kamu sopan, lho.'),
            n('Matanya berbinar, lalu nadanya berubah sedikit serius, seperti senior yang sedang menjelaskan.', E),
            ed("Yang kamu lakuin tadi tuh pas banget. Kamu kasih dia jalan buat benerin salahnya sendiri, tanpa malu di depan antrean. Di sini itu soal <i>miànzi</i>, muka. Banyak orang baru yang langsung bilang 'salah' gitu aja, terus bingung kenapa Āyí-nya jadi jutek sebulan."),
            n('Padahal bahasa Mandarinku cuma sampai <i>bù hǎo yìsi</i>. Sisanya kertas dan senyum.', E),
        ]),
        ('[Diam] Ambil kunci 305 tanpa berkata apa-apa. Urusannya nanti saja.', 'd0_reg_diam', [
            n('Rasanya tidak enak mengoreksi orang yang baru kukenal tiga puluh detik. Aku mengambil kunci itu dan mulai menarik koper.'),
            n('Tapi Edelweiss sudah melirik kartu di tanganku, lalu kertas di tangan satunya. Dia maju setengah langkah.', E),
            *EDEL_FIX,
            TEGUR_LIRIK,
            ed('Kenapa diem aja tadi?'),
            dv('Nggak enak. Baru kenal, masa langsung dikoreksi.'),
            ed('Aku ngerti, aku dulu juga gitu. Tapi kalau diem, nanti malam kamu nyasar masuk kamar orang.'),
            n('Dia menghela napas kecil, lalu tersenyum.', E),
            ed("Diem itu bukan sopan, Dev. Salah tetap harus dibilang. Yang dijaga itu caranya. Tadi kamu dengar kan aku bilang 'mungkin <i>kami</i> yang kurang jelas'? Salahnya aku ambil sedikit, jadi Āyí nggak kehilangan muka di depan antrean. Di sini itu namanya jaga <i>miànzi</i>."),
            n('Aku memutar kunci 503 di jariku. Ternyata ada cara ketiga di antara diam dan menegur. Aku tidak pernah memikirkannya.', E),
        ]),
        ('[Lewat perantara] Bisikkan ke Edelweiss: "Nomornya kebalik. Bisa tolong bilangin?"', 'd0_reg_perantara', [
            dv('(berbisik) Nomornya kebalik. Bisa tolong bilangin?'),
            n('Edelweiss mengangguk kecil, lalu maju setengah langkah dengan senyum yang sudah siap.', E),
            *EDEL_FIX,
            TEGUR_LIRIK,
            ed('Nggak apa-apa kok minta tolong lewat aku. Di sini pakai perantara malah wajar.'),
            n('Lalu, lebih pelan:', E),
            ed("Tapi aku nggak selalu ada, ya. Jadi dengerin baik-baik caraku tadi. Aku bilang 'mungkin <i>kami</i> yang kurang jelas'. Salahnya aku ambil sedikit, jadi Āyí nggak kehilangan muka di depan antrean. Di sini itu namanya jaga <i>miànzi</i>. Besok-besok coba sendiri, mulai dari <i>bù hǎo yìsi</i>."),
            n('Aku mengangguk. Kalimat itu sudah kuhafal sejak di pesawat. Aku cuma belum tahu kalimat itu dipakai untuk apa.', E),
        ]),
    ], sp=''),
    ed('Satu lagi. Baik-baikin Āyí. Paket, jam malam, kunci cadangan kalau kamu kekunci, semua lewat dia. Di sini, kenal baik sama orang yang tepat itu lebih kepake daripada hafal peraturan.'),
    n('Aku menyimpan kalimat itu di kepala, di sebelah <i>bù hǎo yìsi</i>.', E),

    # ADEGAN 4 — KAKI TANGGA
    n('Edelweiss berhenti di anak tangga pertama.', E),
    ed('Aku cuma bisa antar sampai sini. Cewek nggak boleh naik.'),
    n('Aku baru sadar plester di jari telunjukku sudah lepas separuh. Mungkin dari gesekan dengan pegangan koper. Lukanya sudah kering, tidak masalah sebenarnya. Aku baru mau menempelkannya lagi ketika Edelweiss lebih dulu melihat.', E),
    ed('Eh, jarimu kenapa?'),
    c('Luka', [
        ('[Santai] "Nggak apa-apa, cuma kegores dikit di bandara."', 'd0_luka_santai', [
            dv('Nggak apa-apa, cuma kegores dikit di bandara.'),
            ed('Dikit apanya, plesternya udah mau copot gitu.'),
        ]),
        ('[Pasrah-lucu] "Biasa. Kalau ada benda tajam dalam radius lima meter, pasti nemu aku."', 'd0_luka_lucu', [
            dv('Biasa. Kalau ada benda tajam dalam radius lima meter, pasti nemu aku.'),
            n('Dia tertawa, kali ini lebih lepas dari tadi.', E),
            ed('Kamu ngomongnya kayak bangga.'),
        ]),
        ('[Mengalihkan] Masukkan tangan ke saku. "Bukan apa-apa. Lantai lima lewat sini, kan?"', 'd0_luka_alih', [
            n('Aku memasukkan tangan ke saku.', E),
            dv('Bukan apa-apa. Lantai lima lewat sini, kan?'),
            ed('Eh, jangan disembunyiin, sini—'),
            n('Nadanya sempat setengah memerintah, lalu dia sendiri kaget dan buru-buru melunak.', E),
            ed('…Maksudku, boleh lihat nggak?'),
        ]),
    ]),
    n('Dia sudah membuka tas kecilnya sebelum aku sempat bilang apa-apa, lalu mengeluarkan kotak plester berwarna pastel.', E),
    ed('Sini deh, aku bawa plester yang lebih bagus. Ada motifnya lho. Pakai ini aja.'),
    n('Ada sesuatu dalam caranya menyodorkan kotak itu yang terasa tulus, bukan sekadar basa-basi tugas jemput mahasiswa baru. Untuk sesaat aku hampir menerimanya begitu saja.', E),
    n('Tapi tanganku sudah lebih dulu menempelkan plester lamaku kembali, sebelum aku sempat memutuskan apa-apa.', E),
    c('Penolakan', [
        ('[Sopan] "Nggak usah, makasih ya. Aku bawa sendiri kok di tas."', 'd0_tolak_sopan', [
            dv('Nggak usah, makasih ya. Aku bawa sendiri kok di tas.'),
            ed('Oh.'),
            n('Dia menutup kotak plesternya.', E),
            ed('Oke deh kalau gitu.'),
        ]),
        ('[Bercanda] "Tenang, stok plesterku lebih banyak dari apotek. Pengalaman."', 'd0_tolak_canda', [
            dv('Tenang, stok plesterku lebih banyak dari apotek. Pengalaman.'),
            n('Dia tertawa. Tangannya pelan-pelan menutup kotak itu sambil tertawa.', E),
            ed('Iya, iya, Pak Apoteker.'),
        ]),
        ('[Sungkan] "Eh, nggak usah, nggak usah. Ngerepotin. Simpan aja buat kamu."', 'd0_tolak_sungkan', [
            dv('Eh, nggak usah, nggak usah. Ngerepotin. Simpan aja buat kamu.'),
            ed('Nggak ngerepotin kok, aku—'),
            n('Kalimatnya berhenti di tengah. Dia menutup kotaknya.', E),
            ed('…Oke.'),
        ]),
    ]),
    n('Sedetik kemudian dia sudah tersenyum lagi, mengingatkanku soal jam malam, lalu melambai dan berjalan keluar lobi dengan langkah cepatnya yang tadi.', E),
    n('Tapi ada satu hal yang tetap kutangkap. Sepersekian detik sebelum senyum itu kembali, ada jeda yang tidak seharusnya ada di sana.'),
    n('Aku tidak tahu itu apa. Mungkin cuma capek. Aku mengangkat koper ke anak tangga pertama dan mulai naik sendirian.'),

    # ADEGAN 5 — KAMAR 503
    n('Kamar asramaku kosong dan rapi, seperti belum pernah dipakai siapa pun. Mungkin memang belum. Koperku belum kubongkar. Aku duduk di tepi ranjang dengan tas kamera masih tergantung di bahu, terlalu capek bahkan untuk melepasnya.'),
    n('Dari jendela, lampu koridor gedung sebelah masih menyala satu-satu, sama seperti tadi sore. Bedanya, langit di belakangnya sekarang sudah gelap total.'),
    n('Hari ini aku belajar satu aturan. Cara menyampaikan kesalahan orang tanpa membuatnya kehilangan muka. Aturan itu bisa dijelaskan dalam tiga kalimat di kaki tangga.'),
    n('Tapi jeda di senyum Edelweiss tadi, aku tidak tahu itu aturan yang mana.'),
    n('Aku melepas tas kamera, meletakkannya di meja, dan berbaring tanpa mengganti baju. Besok hari pertama yang sesungguhnya.'),
    n('Hal terakhir yang terlintas sebelum aku tertidur bukan gerbang kampus, bukan kamar baru ini, tapi jeda kecil itu. Yang tidak sempat selesai kupikirkan.'),
]

nodes, opts = [], []
nid, oid = [NODE0], [OPT0]

def build(items, after):
    """Assigns ids to items, links each to the next (or `after`), returns the first id."""
    ids = list(range(nid[0], nid[0] + len(items)))
    nid[0] += len(items)
    for i, it in enumerate(items):
        nxt = ids[i + 1] if i + 1 < len(items) else after
        if it[0] == 'c':
            _, title, sp, options = it
            nodes.append((ids[i], '', '', sp, 0, 1, 0))  # titik pilihan: tanpa nama & teks (title hanya untuk penulis)
            for label, flag, branch in options:
                opts.append((oid[0], ids[i], label, build(branch, nxt) if branch else nxt, flag))
                oid[0] += 1
        else:
            speaker, text, sp = ('', it[1], it[2]) if it[0] == 'n' else it[1:]
            nodes.append((ids[i], speaker, text, sp, nxt, 0, 2 if speaker == 'Edelweiss' else 0))
    return ids[0] if ids else after

build(SCRIPT, 0)
nodes.sort()
assert nodes[0][0] == NODE0 and nid[0] <= 9999, nid[0]

q = lambda s: "'" + s.replace("'", "''") + "'"
out = ['-- Prolog Hari 0 (Kedatangan). Digenerate dari day0_kedatangan_vn_v2.md — edit Tools/gen_day0_prologue.py lalu generate ulang.',
       'DELETE FROM tbl_dialogue_options WHERE node_id BETWEEN 9000 AND 9999 OR node_id BETWEEN 10001 AND 10005;',
       'DELETE FROM tbl_dialogue_nodes WHERE node_id BETWEEN 9000 AND 9999 OR node_id BETWEEN 10001 AND 10005;',
       'INSERT INTO tbl_dialogue_nodes (node_id, dialogue_id, dialogue_code, speaker_name, dialogue_text, character_sprite_path, next_dialogue_id, has_options, npc_id, req_language, req_etiquette, priority) VALUES']
rows = []
for (i, spk, txt, sp, nxt, has, npc) in nodes:
    code = 'D_PROLOGUE_01' if i == NODE0 else f'D_PROLOGUE_{i}'
    rows.append(f"({i}, {i}, {q(code)}, {q(spk)}, {q(txt)}, {q(sp)}, {nxt}, {has}, {npc}, 0, 0, 10)")
out.append(',\n'.join(rows) + ';')
out.append('INSERT INTO tbl_dialogue_options (option_id, dialogue_id, node_id, option_text, next_dialogue_id, next_node_id, set_flag_key, set_flag_name, set_flag_value, set_flag_val) VALUES')
out.append(',\n'.join(f"({o}, {n_}, {n_}, {q(t)}, {nx}, {nx}, {q(f)}, {q(f)}, 1, 1)" for (o, n_, t, nx, f) in opts) + ';')
open(sys.argv[1], 'w', encoding='utf-8', newline='\n').write('\n'.join(out) + '\n')
print(len(nodes), 'nodes', len(opts), 'options, ids', NODE0, '-', nid[0] - 1)
