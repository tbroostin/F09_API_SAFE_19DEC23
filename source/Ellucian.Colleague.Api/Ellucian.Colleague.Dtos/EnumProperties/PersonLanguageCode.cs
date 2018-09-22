// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Person language codes based on the ISO 639-3 alpha-3 language code
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonLanguageCode
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// aar
        /// </summary>
        [EnumMember(Value = "aar")]
        aar,

        /// <summary>
        /// abk
        /// </summary>
        [EnumMember(Value = "abk")]
        abk,
        /// <summary>
        /// afr
        /// </summary>
        [EnumMember(Value = "afr")]
        afr,
        /// <summary>
        /// aka
        /// </summary>
        [EnumMember(Value = "aka")]
        aka,
        /// <summary>
        /// alb
        /// </summary>
        [EnumMember(Value = "alb")]
        alb,
        /// <summary>
        /// amh
        /// </summary>
        [EnumMember(Value = "amh")]
        amh,
        /// <summary>
        /// ara
        /// </summary>
        [EnumMember(Value = "ara")]
        ara,
        /// <summary>
        /// 
        /// </summary>arg
        [EnumMember(Value = "arg")]
        arg,
        /// <summary>
        /// arm
        /// </summary>
        [EnumMember(Value = "arm")]
        arm,
        /// <summary>
        /// asm
        /// </summary>
        [EnumMember(Value = "asm")]
        asm,
        /// <summary>
        /// ava
        /// </summary>
        [EnumMember(Value = "ava")]
        ava,
        /// <summary>
        /// ave
        /// </summary>
        [EnumMember(Value = "ave")]
        ave,
        /// <summary>
        /// aym
        /// </summary>
        [EnumMember(Value = "aym")]
        aym,
        /// <summary>
        /// aze
        /// </summary>
        [EnumMember(Value = "aze")]
        aze,
        /// <summary>
        /// bak
        /// </summary>
        [EnumMember(Value = "bak")]
        bak,
        /// <summary>
        /// bam
        /// </summary>
        [EnumMember(Value = "bam")]
        bam,
        /// <summary>
        /// baq
        /// </summary>
        [EnumMember(Value = "baq")]
        baq,
        /// <summary>
        /// bel
        /// </summary>
        [EnumMember(Value = "bel")]
        bel,
        /// <summary>
        /// ben
        /// </summary>
        [EnumMember(Value = "ben")]
        ben,
        /// <summary>
        /// bih
        /// </summary>
        [EnumMember(Value = "bih")]
        bih,
        /// <summary>
        /// bis
        /// </summary>
        [EnumMember(Value = "bis")]
        bis,
        /// <summary>
        /// bos
        /// </summary>
        [EnumMember(Value = "bos")]
        bos,
        /// <summary>
        /// bre
        /// </summary>
        [EnumMember(Value = "bre")]
        bre,
        /// <summary>
        /// bul
        /// </summary>
        [EnumMember(Value = "bul")]
        bul,
        /// <summary>
        /// bur
        /// </summary>
        [EnumMember(Value = "bur")]
        bur,
        /// <summary>
        /// cat
        /// </summary>
        [EnumMember(Value = "cat")]
        cat,
        /// <summary>
        /// cha
        /// </summary>
        [EnumMember(Value = "cha")]
        cha,
        /// <summary>
        /// che
        /// </summary>
        [EnumMember(Value = "che")]
        che,
        /// <summary>
        /// chi
        /// </summary>
        [EnumMember(Value = "chi")]
        chi,
        /// <summary>
        /// chu
        /// </summary>
        [EnumMember(Value = "chu")]
        chu,
        /// <summary>
        /// chv
        /// </summary>
        [EnumMember(Value = "chv")]
        chv,
        /// <summary>
        /// cor
        /// </summary>
        [EnumMember(Value = "cor")]
        cor,
        /// <summary>
        /// cos
        /// </summary>
        [EnumMember(Value = "cos")]
        cos,
        /// <summary>
        /// cre
        /// </summary>
        [EnumMember(Value = "cre")]
        cre,
        /// <summary>
        /// cze
        /// </summary>
        [EnumMember(Value = "cze")]
        cze,
        /// <summary>
        /// dan
        /// </summary>
        [EnumMember(Value = "dan")]
        dan,
        /// <summary>
        /// div
        /// </summary>
        [EnumMember(Value = "div")]
        div,
        /// <summary>
        /// dut
        /// </summary>
        [EnumMember(Value = "dut")]
        dut,
        /// <summary>
        /// dzo
        /// </summary>
        [EnumMember(Value = "dzo")]
        dzo,
        /// <summary>
        /// eng
        /// </summary>
        [EnumMember(Value = "eng")]
        eng,
        /// <summary>
        /// epo
        /// </summary>
        [EnumMember(Value = "epo")]
        epo,
        /// <summary>
        /// est
        /// </summary>
        [EnumMember(Value = "est")]
        est,
        /// <summary>
        /// ewe
        /// </summary>
        [EnumMember(Value = "ewe")]
        ewe,
        /// <summary>
        /// fao
        /// </summary>
        [EnumMember(Value = "fao")]
        fao,
        /// <summary>
        /// fij
        /// </summary>
        [EnumMember(Value = "fij")]
        fij,
        /// <summary>
        /// fin
        /// </summary>
        [EnumMember(Value = "fin")]
        fin,
        /// <summary>
        /// fre
        /// </summary>
        [EnumMember(Value = "fre")]
        fre,
        /// <summary>
        /// fry
        /// </summary>
        [EnumMember(Value = "fry")]
        fry,
        /// <summary>
        /// ful
        /// </summary>
        [EnumMember(Value = "ful")]
        ful,
        /// <summary>
        /// geo
        /// </summary>
        [EnumMember(Value = "geo")]
        geo,
        /// <summary>
        /// ger
        /// </summary>
        [EnumMember(Value = "ger")]
        ger,
        /// <summary>
        /// gla
        /// </summary>
        [EnumMember(Value = "gla")]
        gla,
        /// <summary>
        /// gle
        /// </summary>
        [EnumMember(Value = "gle")]
        gle,
        /// <summary>
        /// glg
        /// </summary>
        [EnumMember(Value = "glg")]
        glg,
        /// <summary>
        /// glv
        /// </summary>
        [EnumMember(Value = "glv")]
        glv,
        /// <summary>
        /// gre
        /// </summary>
        [EnumMember(Value = "gre")]
        gre,
        /// <summary>
        /// grn
        /// </summary>
        [EnumMember(Value = "grn")]
        grn,
        /// <summary>
        /// guj
        /// </summary>
        [EnumMember(Value = "guj")]
        guj,
        /// <summary>
        /// hat
        /// </summary>
        [EnumMember(Value = "hat")]
        hat,
        /// <summary>
        /// hau
        /// </summary>
        [EnumMember(Value = "hau")]
        hau,
        /// <summary>
        /// heb
        /// </summary>
        [EnumMember(Value = "heb")]
        heb,
        /// <summary>
        /// her
        /// </summary>
        [EnumMember(Value = "her")]
        her,
        /// <summary>
        /// hin
        /// </summary>
        [EnumMember(Value = "hin")]
        hin,
        /// <summary>
        /// hmo
        /// </summary>
        [EnumMember(Value = "hmo")]
        hmo,
        /// <summary>
        /// hrv
        /// </summary>
        [EnumMember(Value = "hrv")]
        hrv,
        /// <summary>
        /// hun
        /// </summary>
        [EnumMember(Value = "hun")]
        hun,
        /// <summary>
        /// ibo
        /// </summary>
        [EnumMember(Value = "ibo")]
        ibo,
        /// <summary>
        /// ice
        /// </summary>
        [EnumMember(Value = "ice")]
        ice,
        /// <summary>
        /// ido
        /// </summary>
        [EnumMember(Value = "ido")]
        ido,
        /// <summary>
        /// iii
        /// </summary>
        [EnumMember(Value = "iii")]
        iii,
        /// <summary>
        /// iku
        /// </summary>
        [EnumMember(Value = "iku")]
        iku,
        /// <summary>
        /// ile
        /// </summary>
        [EnumMember(Value = "ile")]
        ile,
        /// <summary>
        /// ina
        /// </summary>
        [EnumMember(Value = "ina")]
        ina,
        /// <summary>
        /// ind
        /// </summary>
        [EnumMember(Value = "ind")]
        ind,
        /// <summary>
        /// ipk
        /// </summary>
        [EnumMember(Value = "ipk")]
        ipk,
        /// <summary>
        /// ita
        /// </summary>
        [EnumMember(Value = "ita")]
        ita,
        /// <summary>
        /// jav
        /// </summary>
        [EnumMember(Value = "jav")]
        jav,
        /// <summary>
        /// jpn
        /// </summary>
        [EnumMember(Value = "jpn")]
        jpn,
        /// <summary>
        /// kal
        /// </summary>
        [EnumMember(Value = "kal")]
        kal,
        /// <summary>
        /// kan
        /// </summary>
        [EnumMember(Value = "kan")]
        kan,
        /// <summary>
        /// kas
        /// </summary>
        [EnumMember(Value = "kas")]
        kas,
        /// <summary>
        /// kau
        /// </summary>
        [EnumMember(Value = "kau")]
        kau,
        /// <summary>
        /// kaz
        /// </summary>
        [EnumMember(Value = "kaz")]
        kaz,
        /// <summary>
        /// khm
        /// </summary>
        [EnumMember(Value = "khm")]
        khm,
        /// <summary>
        /// kik
        /// </summary>
        [EnumMember(Value = "kik")]
        kik,
        /// <summary>
        /// kin
        /// </summary>
        [EnumMember(Value = "kin")]
        kin,
        /// <summary>
        /// kir
        /// </summary>
        [EnumMember(Value = "kir")]
        kir,
        /// <summary>
        /// kom
        /// </summary>
        [EnumMember(Value = "kom")]
        kom,
        /// <summary>
        /// kon
        /// </summary>
        [EnumMember(Value = "kon")]
        kon,
        /// <summary>
        /// kor
        /// </summary>
        [EnumMember(Value = "kor")]
        kor,
        /// <summary>
        /// kua
        /// </summary>
        [EnumMember(Value = "kua")]
        kua,
        /// <summary>
        /// kur
        /// </summary>
        [EnumMember(Value = "kur")]
        kur,
        /// <summary>
        /// lao
        /// </summary>
        [EnumMember(Value = "lao")]
        lao,
        /// <summary>
        /// lat
        /// </summary>
        [EnumMember(Value = "lat")]
        lat,
        /// <summary>
        /// lav
        /// </summary>
        [EnumMember(Value = "lav")]
        lav,
        /// <summary>
        /// lim
        /// </summary>
        [EnumMember(Value = "lim")]
        lim,
        /// <summary>
        /// lin
        /// </summary>
        [EnumMember(Value = "lin")]
        lin,
        /// <summary>
        /// lit
        /// </summary>
        [EnumMember(Value = "lit")]
        lit,
        /// <summary>
        /// ltz
        /// </summary>
        [EnumMember(Value = "ltz")]
        ltz,
        /// <summary>
        /// lub
        /// </summary>
        [EnumMember(Value = "lub")]
        lub,
        /// <summary>
        /// lug
        /// </summary>
        [EnumMember(Value = "lug")]
        lug,
        /// <summary>
        /// mac
        /// </summary>
        [EnumMember(Value = "mac")]
        mac,
        /// <summary>
        /// mah
        /// </summary>
        [EnumMember(Value = "mah")]
        mah,
        /// <summary>
        /// mal
        /// </summary>
        [EnumMember(Value = "mal")]
        mal,
        /// <summary>
        /// mao
        /// </summary>
        [EnumMember(Value = "mao")]
        mao,
        /// <summary>
        /// mar
        /// </summary>
        [EnumMember(Value = "mar")]
        mar,
        /// <summary>
        /// may
        /// </summary>
        [EnumMember(Value = "may")]
        may,
        /// <summary>
        /// mlg
        /// </summary>
        [EnumMember(Value = "mlg")]
        mlg,
        /// <summary>
        /// mlt
        /// </summary>
        [EnumMember(Value = "mlt")]
        mlt,
        /// <summary>
        /// mon
        /// </summary>
        [EnumMember(Value = "mon")]
        mon,
        /// <summary>
        /// nau
        /// </summary>
        [EnumMember(Value = "nau")]
        nau,
        /// <summary>
        /// nav
        /// </summary>
        [EnumMember(Value = "nav")]
        nav,
        /// <summary>
        /// nbl
        /// </summary>
        [EnumMember(Value = "nbl")]
        nbl,
        /// <summary>
        /// nde
        /// </summary>
        [EnumMember(Value = "nde")]
        nde,
        /// <summary>
        /// ndo
        /// </summary>
        [EnumMember(Value = "ndo")]
        ndo,
        /// <summary>
        /// nep
        /// </summary>
        [EnumMember(Value = "nep")]
        nep,
        /// <summary>
        /// nno
        /// </summary>
        [EnumMember(Value = "nno")]
        nno,
        /// <summary>
        /// nob
        /// </summary>
        [EnumMember(Value = "nob")]
        nob,
        /// <summary>
        /// nor
        /// </summary>
        [EnumMember(Value = "nor")]
        nor,
        /// <summary>
        /// nya
        /// </summary>
        [EnumMember(Value = "nya")]
        nya,
        /// <summary>
        /// oci
        /// </summary>
        [EnumMember(Value = "oci")]
        oci,
        /// <summary>
        /// oji
        /// </summary>
        [EnumMember(Value = "oji")]
        oji,
        /// <summary>
        /// ori
        /// </summary>
        [EnumMember(Value = "ori")]
        ori,
        /// <summary>
        /// orm
        /// </summary>
        [EnumMember(Value = "orm")]
        orm,
        /// <summary>
        /// oss
        /// </summary>
        [EnumMember(Value = "oss")]
        oss,
        /// <summary>
        /// pan
        /// </summary>
        [EnumMember(Value = "pan")]
        pan,
        /// <summary>
        /// per
        /// </summary>
        [EnumMember(Value = "per")]
        per,
        /// <summary>
        /// pli
        /// </summary>
        [EnumMember(Value = "pli")]
        pli,
        /// <summary>
        /// pol
        /// </summary>
        [EnumMember(Value = "pol")]
        pol,
        /// <summary>
        /// por
        /// </summary>
        [EnumMember(Value = "por")]
        por,
        /// <summary>
        /// pus
        /// </summary>
        [EnumMember(Value = "pus")]
        pus,
        /// <summary>
        /// que
        /// </summary>
        [EnumMember(Value = "que")]
        que,
        /// <summary>
        /// roh
        /// </summary>
        [EnumMember(Value = "roh")]
        roh,
        /// <summary>
        /// rum
        /// </summary>
        [EnumMember(Value = "rum")]
        rum,
        /// <summary>
        /// run
        /// </summary>
        [EnumMember(Value = "run")]
        run,
        /// <summary>
        /// rus
        /// </summary>
        [EnumMember(Value = "rus")]
        rus,
        /// <summary>
        /// sag
        /// </summary>
        [EnumMember(Value = "sag")]
        sag,
        /// <summary>
        /// san
        /// </summary>
        [EnumMember(Value = "san")]
        san,
        /// <summary>
        /// sin
        /// </summary>
        [EnumMember(Value = "sin")]
        sin,
        /// <summary>
        /// slo
        /// </summary>
        [EnumMember(Value = "slo")]
        slo,
        /// <summary>
        /// slv
        /// </summary>
        [EnumMember(Value = "slv")]
        slv,
        /// <summary>
        /// sme
        /// </summary>
        [EnumMember(Value = "sme")]
        sme,
        /// <summary>
        /// smo
        /// </summary>
        [EnumMember(Value = "smo")]
        smo,
        /// <summary>
        /// sna
        /// </summary>
        [EnumMember(Value = "sna")]
        sna,
        /// <summary>
        /// snd
        /// </summary>
        [EnumMember(Value = "snd")]
        snd,
        /// <summary>
        /// som
        /// </summary>
        [EnumMember(Value = "som")]
        som,
        /// <summary>
        /// sot
        /// </summary>
        [EnumMember(Value = "sot")]
        sot,
        /// <summary>
        /// spa
        /// </summary>
        [EnumMember(Value = "spa")]
        spa,
        /// <summary>
        /// srd
        /// </summary>
        [EnumMember(Value = "srd")]
        srd,
        /// <summary>
        /// srp
        /// </summary>
        [EnumMember(Value = "srp")]
        srp,
        /// <summary>
        /// ssw
        /// </summary>
        [EnumMember(Value = "ssw")]
        ssw,
        /// <summary>
        /// sun
        /// </summary>
        [EnumMember(Value = "sun")]
        sun,
        /// <summary>
        /// swa
        /// </summary>
        [EnumMember(Value = "swa")]
        swa,
        /// <summary>
        /// swe
        /// </summary>
        [EnumMember(Value = "swe")]
        swe,
        /// <summary>
        /// tah
        /// </summary>
        [EnumMember(Value = "tah")]
        tah,
        /// <summary>
        /// tam
        /// </summary>
        [EnumMember(Value = "tam")]
        tam,
        /// <summary>
        /// tat
        /// </summary>
        [EnumMember(Value = "tat")]
        tat,
        /// <summary>
        /// tel
        /// </summary>
        [EnumMember(Value = "tel")]
        tel,
        /// <summary>
        /// tgk
        /// </summary>
        [EnumMember(Value = "tgk")]
        tgk,
        /// <summary>
        /// tgl
        /// </summary>
        [EnumMember(Value = "tgl")]
        tgl,
        /// <summary>
        /// tha
        /// </summary>
        [EnumMember(Value = "tha")]
        tha,
        /// <summary>
        /// tib
        /// </summary>
        [EnumMember(Value = "tib")]
        tib,
        /// <summary>
        /// tir
        /// </summary>
        [EnumMember(Value = "tir")]
        tir,
        /// <summary>
        /// ton
        /// </summary>
        [EnumMember(Value = "ton")]
        ton,
        /// <summary>
        /// tsn
        /// </summary>
        [EnumMember(Value = "tsn")]
        tsn,
        /// <summary>
        /// tso
        /// </summary>
        [EnumMember(Value = "tso")]
        tso,
        /// <summary>
        /// tuk
        /// </summary>
        [EnumMember(Value = "tuk")]
        tuk,
        /// <summary>
        /// tur
        /// </summary>
        [EnumMember(Value = "tur")]
        tur,
        /// <summary>
        /// twi
        /// </summary>
        [EnumMember(Value = "twi")]
        twi,
        /// <summary>
        /// uig
        /// </summary>
        [EnumMember(Value = "uig")]
        uig,
        /// <summary>
        /// ukr
        /// </summary>
        [EnumMember(Value = "ukr")]
        ukr,
        /// <summary>
        /// urd
        /// </summary>
        [EnumMember(Value = "urd")]
        urd,
        /// <summary>
        /// uzb
        /// </summary>
        [EnumMember(Value = "uzb")]
        uzb,
        /// <summary>
        /// ven
        /// </summary>
        [EnumMember(Value = "ven")]
        ven,
        /// <summary>
        /// vie
        /// </summary>
        [EnumMember(Value = "vie")]
        vie,
        /// <summary>
        /// vol
        /// </summary>
        [EnumMember(Value = "vol")]
        vol,
        /// <summary>
        /// wel
        /// </summary>
        [EnumMember(Value = "wel")]
        wel,
        /// <summary>
        /// wln
        /// </summary>
        [EnumMember(Value = "wln")]
        wln,
        /// <summary>
        /// wol
        /// </summary>
        [EnumMember(Value = "wol")]
        wol,
        /// <summary>
        /// xho
        /// </summary>
        [EnumMember(Value = "xho")]
        xho,
        /// <summary>
        /// yid
        /// </summary>
        [EnumMember(Value = "yid")]
        yid,
        /// <summary>
        /// yor
        /// </summary>
        [EnumMember(Value = "yor")]
        yor,

        /// <summary>
        /// zha
        /// </summary>
        [EnumMember(Value = "zha")]
        zha,
        /// <summary>
        /// zul
        /// </summary>
        [EnumMember(Value = "zul")]
        zul
    }
}
