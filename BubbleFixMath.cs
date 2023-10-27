//using ShinyShoe;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
namespace MagmaDataMiner
{
    public static class BubbleFixMath
    {
        public static readonly BubbleFix PI;

        public static readonly BubbleFix E;

        public static readonly BubbleFix Deg2Rad;

        public static readonly BubbleFix Rad2Deg;

        private static BubbleFix _log2_E;

        private static BubbleFix _log2_10;

        private static BubbleFix _ln2;

        private static BubbleFix _log10_2;

        private static BubbleFix[] _quarterSine;

        private static BubbleFix[] _cordicAngles;

        private static BubbleFix[] _cordicGains;

        private static BubbleFixConst _piConst;

        private static BubbleFixConst _eConst;

        private static BubbleFixConst _log2_EConst;

        private static BubbleFixConst _log2_10Const;

        private static BubbleFixConst _ln2Const;

        private static BubbleFixConst _log10_2Const;

        private const int _quarterSineResPower = 2;

        private static BubbleFixConst[] _quarterSineConsts;

        private static BubbleFixConst[] _cordicAngleConsts;

        private static BubbleFixConst[] _cordicGainConsts;

        private static BubbleFixConst[] _invFactConsts;

        static BubbleFixMath()
        {
            _piConst = new BubbleFixConst(13493037705L);
            _eConst = new BubbleFixConst(11674931555L);
            _log2_EConst = new BubbleFixConst(6196328019L);
            _log2_10Const = new BubbleFixConst(14267572527L);
            _ln2Const = new BubbleFixConst(2977044472L);
            _log10_2Const = new BubbleFixConst(1292913986L);
            _quarterSineConsts = new BubbleFixConst[361]
            {
                new BubbleFixConst(0L),
                new BubbleFixConst(18740271L),
                new BubbleFixConst(37480185L),
                new BubbleFixConst(56219385L),
                new BubbleFixConst(74957515L),
                new BubbleFixConst(93694218L),
                new BubbleFixConst(112429137L),
                new BubbleFixConst(131161916L),
                new BubbleFixConst(149892197L),
                new BubbleFixConst(168619625L),
                new BubbleFixConst(187343842L),
                new BubbleFixConst(206064493L),
                new BubbleFixConst(224781220L),
                new BubbleFixConst(243493669L),
                new BubbleFixConst(262201481L),
                new BubbleFixConst(280904301L),
                new BubbleFixConst(299601773L),
                new BubbleFixConst(318293542L),
                new BubbleFixConst(336979250L),
                new BubbleFixConst(355658543L),
                new BubbleFixConst(374331065L),
                new BubbleFixConst(392996460L),
                new BubbleFixConst(411654373L),
                new BubbleFixConst(430304448L),
                new BubbleFixConst(448946331L),
                new BubbleFixConst(467579667L),
                new BubbleFixConst(486204101L),
                new BubbleFixConst(504819278L),
                new BubbleFixConst(523424844L),
                new BubbleFixConst(542020445L),
                new BubbleFixConst(560605727L),
                new BubbleFixConst(579180335L),
                new BubbleFixConst(597743917L),
                new BubbleFixConst(616296119L),
                new BubbleFixConst(634836587L),
                new BubbleFixConst(653364969L),
                new BubbleFixConst(671880911L),
                new BubbleFixConst(690384062L),
                new BubbleFixConst(708874069L),
                new BubbleFixConst(727350581L),
                new BubbleFixConst(745813244L),
                new BubbleFixConst(764261708L),
                new BubbleFixConst(782695622L),
                new BubbleFixConst(801114635L),
                new BubbleFixConst(819518395L),
                new BubbleFixConst(837906553L),
                new BubbleFixConst(856278758L),
                new BubbleFixConst(874634661L),
                new BubbleFixConst(892973913L),
                new BubbleFixConst(911296163L),
                new BubbleFixConst(929601063L),
                new BubbleFixConst(947888266L),
                new BubbleFixConst(966157422L),
                new BubbleFixConst(984408183L),
                new BubbleFixConst(1002640203L),
                new BubbleFixConst(1020853134L),
                new BubbleFixConst(1039046630L),
                new BubbleFixConst(1057220343L),
                new BubbleFixConst(1075373929L),
                new BubbleFixConst(1093507041L),
                new BubbleFixConst(1111619334L),
                new BubbleFixConst(1129710464L),
                new BubbleFixConst(1147780085L),
                new BubbleFixConst(1165827855L),
                new BubbleFixConst(1183853429L),
                new BubbleFixConst(1201856464L),
                new BubbleFixConst(1219836617L),
                new BubbleFixConst(1237793546L),
                new BubbleFixConst(1255726910L),
                new BubbleFixConst(1273636366L),
                new BubbleFixConst(1291521575L),
                new BubbleFixConst(1309382194L),
                new BubbleFixConst(1327217885L),
                new BubbleFixConst(1345028307L),
                new BubbleFixConst(1362813122L),
                new BubbleFixConst(1380571991L),
                new BubbleFixConst(1398304576L),
                new BubbleFixConst(1416010539L),
                new BubbleFixConst(1433689544L),
                new BubbleFixConst(1451341253L),
                new BubbleFixConst(1468965330L),
                new BubbleFixConst(1486561441L),
                new BubbleFixConst(1504129249L),
                new BubbleFixConst(1521668421L),
                new BubbleFixConst(1539178623L),
                new BubbleFixConst(1556659521L),
                new BubbleFixConst(1574110783L),
                new BubbleFixConst(1591532075L),
                new BubbleFixConst(1608923068L),
                new BubbleFixConst(1626283428L),
                new BubbleFixConst(1643612827L),
                new BubbleFixConst(1660910933L),
                new BubbleFixConst(1678177418L),
                new BubbleFixConst(1695411953L),
                new BubbleFixConst(1712614210L),
                new BubbleFixConst(1729783862L),
                new BubbleFixConst(1746920580L),
                new BubbleFixConst(1764024040L),
                new BubbleFixConst(1781093915L),
                new BubbleFixConst(1798129881L),
                new BubbleFixConst(1815131613L),
                new BubbleFixConst(1832098787L),
                new BubbleFixConst(1849031081L),
                new BubbleFixConst(1865928172L),
                new BubbleFixConst(1882789739L),
                new BubbleFixConst(1899615460L),
                new BubbleFixConst(1916405015L),
                new BubbleFixConst(1933158084L),
                new BubbleFixConst(1949874349L),
                new BubbleFixConst(1966553491L),
                new BubbleFixConst(1983195193L),
                new BubbleFixConst(1999799137L),
                new BubbleFixConst(2016365009L),
                new BubbleFixConst(2032892491L),
                new BubbleFixConst(2049381270L),
                new BubbleFixConst(2065831032L),
                new BubbleFixConst(2082241464L),
                new BubbleFixConst(2098612252L),
                new BubbleFixConst(2114943086L),
                new BubbleFixConst(2131233655L),
                new BubbleFixConst(2147483648L),
                new BubbleFixConst(2163692756L),
                new BubbleFixConst(2179860670L),
                new BubbleFixConst(2195987083L),
                new BubbleFixConst(2212071688L),
                new BubbleFixConst(2228114178L),
                new BubbleFixConst(2244114248L),
                new BubbleFixConst(2260071593L),
                new BubbleFixConst(2275985909L),
                new BubbleFixConst(2291856895L),
                new BubbleFixConst(2307684246L),
                new BubbleFixConst(2323467662L),
                new BubbleFixConst(2339206844L),
                new BubbleFixConst(2354901489L),
                new BubbleFixConst(2370551301L),
                new BubbleFixConst(2386155981L),
                new BubbleFixConst(2401715233L),
                new BubbleFixConst(2417228758L),
                new BubbleFixConst(2432696264L),
                new BubbleFixConst(2448117454L),
                new BubbleFixConst(2463492036L),
                new BubbleFixConst(2478819716L),
                new BubbleFixConst(2494100203L),
                new BubbleFixConst(2509333207L),
                new BubbleFixConst(2524518436L),
                new BubbleFixConst(2539655602L),
                new BubbleFixConst(2554744416L),
                new BubbleFixConst(2569784592L),
                new BubbleFixConst(2584775843L),
                new BubbleFixConst(2599717883L),
                new BubbleFixConst(2614610429L),
                new BubbleFixConst(2629453196L),
                new BubbleFixConst(2644245902L),
                new BubbleFixConst(2658988265L),
                new BubbleFixConst(2673680006L),
                new BubbleFixConst(2688320843L),
                new BubbleFixConst(2702910498L),
                new BubbleFixConst(2717448694L),
                new BubbleFixConst(2731935154L),
                new BubbleFixConst(2746369601L),
                new BubbleFixConst(2760751762L),
                new BubbleFixConst(2775081362L),
                new BubbleFixConst(2789358128L),
                new BubbleFixConst(2803581789L),
                new BubbleFixConst(2817752074L),
                new BubbleFixConst(2831868713L),
                new BubbleFixConst(2845931437L),
                new BubbleFixConst(2859939978L),
                new BubbleFixConst(2873894071L),
                new BubbleFixConst(2887793449L),
                new BubbleFixConst(2901637847L),
                new BubbleFixConst(2915427003L),
                new BubbleFixConst(2929160652L),
                new BubbleFixConst(2942838535L),
                new BubbleFixConst(2956460391L),
                new BubbleFixConst(2970025959L),
                new BubbleFixConst(2983534983L),
                new BubbleFixConst(2996987204L),
                new BubbleFixConst(3010382368L),
                new BubbleFixConst(3023720217L),
                new BubbleFixConst(3037000500L),
                new BubbleFixConst(3050222962L),
                new BubbleFixConst(3063387353L),
                new BubbleFixConst(3076493421L),
                new BubbleFixConst(3089540917L),
                new BubbleFixConst(3102529593L),
                new BubbleFixConst(3115459201L),
                new BubbleFixConst(3128329495L),
                new BubbleFixConst(3141140230L),
                new BubbleFixConst(3153891163L),
                new BubbleFixConst(3166582050L),
                new BubbleFixConst(3179212649L),
                new BubbleFixConst(3191782722L),
                new BubbleFixConst(3204292027L),
                new BubbleFixConst(3216740327L),
                new BubbleFixConst(3229127385L),
                new BubbleFixConst(3241452965L),
                new BubbleFixConst(3253716833L),
                new BubbleFixConst(3265918754L),
                new BubbleFixConst(3278058497L),
                new BubbleFixConst(3290135830L),
                new BubbleFixConst(3302150525L),
                new BubbleFixConst(3314102350L),
                new BubbleFixConst(3325991081L),
                new BubbleFixConst(3337816489L),
                new BubbleFixConst(3349578350L),
                new BubbleFixConst(3361276439L),
                new BubbleFixConst(3372910535L),
                new BubbleFixConst(3384480416L),
                new BubbleFixConst(3395985861L),
                new BubbleFixConst(3407426651L),
                new BubbleFixConst(3418802568L),
                new BubbleFixConst(3430113397L),
                new BubbleFixConst(3441358921L),
                new BubbleFixConst(3452538927L),
                new BubbleFixConst(3463653201L),
                new BubbleFixConst(3474701533L),
                new BubbleFixConst(3485683711L),
                new BubbleFixConst(3496599527L),
                new BubbleFixConst(3507448772L),
                new BubbleFixConst(3518231241L),
                new BubbleFixConst(3528946727L),
                new BubbleFixConst(3539595028L),
                new BubbleFixConst(3550175940L),
                new BubbleFixConst(3560689261L),
                new BubbleFixConst(3571134792L),
                new BubbleFixConst(3581512334L),
                new BubbleFixConst(3591821689L),
                new BubbleFixConst(3602062661L),
                new BubbleFixConst(3612235055L),
                new BubbleFixConst(3622338677L),
                new BubbleFixConst(3632373336L),
                new BubbleFixConst(3642338838L),
                new BubbleFixConst(3652234996L),
                new BubbleFixConst(3662061621L),
                new BubbleFixConst(3671818526L),
                new BubbleFixConst(3681505524L),
                new BubbleFixConst(3691122431L),
                new BubbleFixConst(3700669065L),
                new BubbleFixConst(3710145244L),
                new BubbleFixConst(3719550787L),
                new BubbleFixConst(3728885515L),
                new BubbleFixConst(3738149250L),
                new BubbleFixConst(3747341816L),
                new BubbleFixConst(3756463039L),
                new BubbleFixConst(3765512743L),
                new BubbleFixConst(3774490758L),
                new BubbleFixConst(3783396912L),
                new BubbleFixConst(3792231035L),
                new BubbleFixConst(3800992960L),
                new BubbleFixConst(3809682520L),
                new BubbleFixConst(3818299548L),
                new BubbleFixConst(3826843882L),
                new BubbleFixConst(3835315358L),
                new BubbleFixConst(3843713815L),
                new BubbleFixConst(3852039094L),
                new BubbleFixConst(3860291035L),
                new BubbleFixConst(3868469481L),
                new BubbleFixConst(3876574278L),
                new BubbleFixConst(3884605270L),
                new BubbleFixConst(3892562305L),
                new BubbleFixConst(3900445232L),
                new BubbleFixConst(3908253899L),
                new BubbleFixConst(3915988159L),
                new BubbleFixConst(3923647864L),
                new BubbleFixConst(3931232868L),
                new BubbleFixConst(3938743028L),
                new BubbleFixConst(3946178199L),
                new BubbleFixConst(3953538241L),
                new BubbleFixConst(3960823014L),
                new BubbleFixConst(3968032378L),
                new BubbleFixConst(3975166196L),
                new BubbleFixConst(3982224333L),
                new BubbleFixConst(3989206654L),
                new BubbleFixConst(3996113026L),
                new BubbleFixConst(4002943318L),
                new BubbleFixConst(4009697400L),
                new BubbleFixConst(4016375143L),
                new BubbleFixConst(4022976420L),
                new BubbleFixConst(4029501105L),
                new BubbleFixConst(4035949075L),
                new BubbleFixConst(4042320205L),
                new BubbleFixConst(4048614376L),
                new BubbleFixConst(4054831467L),
                new BubbleFixConst(4060971360L),
                new BubbleFixConst(4067033938L),
                new BubbleFixConst(4073019085L),
                new BubbleFixConst(4078926688L),
                new BubbleFixConst(4084756634L),
                new BubbleFixConst(4090508812L),
                new BubbleFixConst(4096183113L),
                new BubbleFixConst(4101779428L),
                new BubbleFixConst(4107297652L),
                new BubbleFixConst(4112737678L),
                new BubbleFixConst(4118099404L),
                new BubbleFixConst(4123382727L),
                new BubbleFixConst(4128587547L),
                new BubbleFixConst(4133713764L),
                new BubbleFixConst(4138761282L),
                new BubbleFixConst(4143730003L),
                new BubbleFixConst(4148619834L),
                new BubbleFixConst(4153430681L),
                new BubbleFixConst(4158162453L),
                new BubbleFixConst(4162815059L),
                new BubbleFixConst(4167388412L),
                new BubbleFixConst(4171882423L),
                new BubbleFixConst(4176297008L),
                new BubbleFixConst(4180632082L),
                new BubbleFixConst(4184887562L),
                new BubbleFixConst(4189063369L),
                new BubbleFixConst(4193159422L),
                new BubbleFixConst(4197175643L),
                new BubbleFixConst(4201111956L),
                new BubbleFixConst(4204968286L),
                new BubbleFixConst(4208744559L),
                new BubbleFixConst(4212440704L),
                new BubbleFixConst(4216056650L),
                new BubbleFixConst(4219592328L),
                new BubbleFixConst(4223047672L),
                new BubbleFixConst(4226422614L),
                new BubbleFixConst(4229717092L),
                new BubbleFixConst(4232931042L),
                new BubbleFixConst(4236064403L),
                new BubbleFixConst(4239117116L),
                new BubbleFixConst(4242089121L),
                new BubbleFixConst(4244980364L),
                new BubbleFixConst(4247790788L),
                new BubbleFixConst(4250520341L),
                new BubbleFixConst(4253168970L),
                new BubbleFixConst(4255736624L),
                new BubbleFixConst(4258223255L),
                new BubbleFixConst(4260628816L),
                new BubbleFixConst(4262953261L),
                new BubbleFixConst(4265196545L),
                new BubbleFixConst(4267358626L),
                new BubbleFixConst(4269439463L),
                new BubbleFixConst(4271439016L),
                new BubbleFixConst(4273357246L),
                new BubbleFixConst(4275194119L),
                new BubbleFixConst(4276949597L),
                new BubbleFixConst(4278623649L),
                new BubbleFixConst(4280216242L),
                new BubbleFixConst(4281727345L),
                new BubbleFixConst(4283156931L),
                new BubbleFixConst(4284504972L),
                new BubbleFixConst(4285771441L),
                new BubbleFixConst(4286956316L),
                new BubbleFixConst(4288059574L),
                new BubbleFixConst(4289081193L),
                new BubbleFixConst(4290021154L),
                new BubbleFixConst(4290879439L),
                new BubbleFixConst(4291656032L),
                new BubbleFixConst(4292350918L),
                new BubbleFixConst(4292964084L),
                new BubbleFixConst(4293495518L),
                new BubbleFixConst(4293945210L),
                new BubbleFixConst(4294313152L),
                new BubbleFixConst(4294599336L),
                new BubbleFixConst(4294803757L),
                new BubbleFixConst(4294926411L),
                new BubbleFixConst(4294967296L)
            };
            _cordicAngleConsts = new BubbleFixConst[24]
            {
                new BubbleFixConst(193273528320L),
                new BubbleFixConst(114096026022L),
                new BubbleFixConst(60285206653L),
                new BubbleFixConst(30601712202L),
                new BubbleFixConst(15360239180L),
                new BubbleFixConst(7687607525L),
                new BubbleFixConst(3844741810L),
                new BubbleFixConst(1922488225L),
                new BubbleFixConst(961258780L),
                new BubbleFixConst(480631223L),
                new BubbleFixConst(240315841L),
                new BubbleFixConst(120157949L),
                new BubbleFixConst(60078978L),
                new BubbleFixConst(30039490L),
                new BubbleFixConst(15019745L),
                new BubbleFixConst(7509872L),
                new BubbleFixConst(3754936L),
                new BubbleFixConst(1877468L),
                new BubbleFixConst(938734L),
                new BubbleFixConst(469367L),
                new BubbleFixConst(234684L),
                new BubbleFixConst(117342L),
                new BubbleFixConst(58671L),
                new BubbleFixConst(29335L)
            };
            _cordicGainConsts = new BubbleFixConst[24]
            {
                new BubbleFixConst(3037000500L),
                new BubbleFixConst(2716375826L),
                new BubbleFixConst(2635271635L),
                new BubbleFixConst(2614921743L),
                new BubbleFixConst(2609829388L),
                new BubbleFixConst(2608555990L),
                new BubbleFixConst(2608237621L),
                new BubbleFixConst(2608158028L),
                new BubbleFixConst(2608138129L),
                new BubbleFixConst(2608133154L),
                new BubbleFixConst(2608131911L),
                new BubbleFixConst(2608131600L),
                new BubbleFixConst(2608131522L),
                new BubbleFixConst(2608131503L),
                new BubbleFixConst(2608131498L),
                new BubbleFixConst(2608131497L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L),
                new BubbleFixConst(2608131496L)
            };
            _invFactConsts = new BubbleFixConst[14]
            {
                new BubbleFixConst(4294967296L),
                new BubbleFixConst(4294967296L),
                new BubbleFixConst(2147483648L),
                new BubbleFixConst(715827883L),
                new BubbleFixConst(178956971L),
                new BubbleFixConst(35791394L),
                new BubbleFixConst(5965232L),
                new BubbleFixConst(852176L),
                new BubbleFixConst(106522L),
                new BubbleFixConst(11836L),
                new BubbleFixConst(1184L),
                new BubbleFixConst(108L),
                new BubbleFixConst(9L),
                new BubbleFixConst(1L)
            };
            if (_quarterSineConsts.Length != 361)
            {
                throw new Exception("_quarterSineConst.Length must be 90 * 2^(_quarterSineResPower) + 1.");
            }

            PI = _piConst;
            E = _eConst;
            Deg2Rad = new BubbleFix((float)Math.PI / 180f);
            Rad2Deg = new BubbleFix(57.29578f);
            _log2_E = _log2_EConst;
            _log2_10 = _log2_10Const;
            _ln2 = _ln2Const;
            _log10_2 = _log10_2Const;
            _quarterSine = Array.ConvertAll(_quarterSineConsts, (Converter<BubbleFixConst, BubbleFix>)((BubbleFixConst c) => c));
            _cordicAngles = Array.ConvertAll(_cordicAngleConsts, (Converter<BubbleFixConst, BubbleFix>)((BubbleFixConst c) => c));
            _cordicGains = Array.ConvertAll(_cordicGainConsts, (Converter<BubbleFixConst, BubbleFix>)((BubbleFixConst c) => c));
        }

        public static BubbleFix Abs(BubbleFix value)
        {
            if (value.Raw >= 0)
            {
                return value;
            }

            return new BubbleFix(-value.Raw);
        }

        public static BubbleFix Sign(BubbleFix value)
        {
            if (value < 0)
            {
                return -1;
            }

            if (value > 0)
            {
                return 1;
            }

            return 0;
        }

        public static BubbleFix Ceiling(BubbleFix value)
        {
            return new BubbleFix((value.Raw + 4095) & -4096);
        }

        public static BubbleFix Floor(BubbleFix value)
        {
            return new BubbleFix(value.Raw & -4096);
        }

        public static BubbleFix Truncate(BubbleFix value)
        {
            if (value < 0)
            {
                return new BubbleFix((value.Raw + 4096) & -4096);
            }

            return new BubbleFix(value.Raw & -4096);
        }

        public static BubbleFix Round(BubbleFix value, bool clampMinMax = false)
        {
            if (clampMinMax && value >= BubbleFix.MaxValue)
            {
                return BubbleFix.MaxValue;
            }

            if (clampMinMax && value <= BubbleFix.MinValue)
            {
                return BubbleFix.MinValue;
            }

            return new BubbleFix((value.Raw + 2048) & -4096);
        }

        public static BubbleFix Min(BubbleFix v1, BubbleFix v2)
        {
            if (!(v1 < v2))
            {
                return v2;
            }

            return v1;
        }

        public static BubbleFix Max(BubbleFix v1, BubbleFix v2)
        {
            if (!(v1 > v2))
            {
                return v2;
            }

            return v1;
        }

        public static BubbleFix Sqrt(BubbleFix value)
        {
            if (value.Raw < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Value must be non-negative.");
            }

            if (value.Raw == 0)
            {
                return 0;
            }

            return new BubbleFix((int)(SqrtULong((ulong)((long)value.Raw << 14)) + 1) >> 1);
        }

        internal static uint SqrtULong(ulong N)
        {
            ulong num = 8388608uL;
            while (true)
            {
                ulong num2 = num + N / num >> 1;
                if (num2 >= num)
                {
                    break;
                }

                num = num2;
            }

            return (uint)num;
        }

        public static BubbleFix Sin(BubbleFix degrees)
        {
            return CosRaw(degrees.Raw - 368640);
        }

        public static BubbleFix Cos(BubbleFix degrees)
        {
            return CosRaw(degrees.Raw);
        }

        public static BubbleFixVec2 RotateBubbleFixVec2(BubbleFixVec2 direction, BubbleFix degrees)
        {
            BubbleFix fix = Cos(degrees);
            BubbleFix fix2 = Sin(degrees);
            return new BubbleFixVec2(fix * direction.X - fix2 * direction.Y, fix2 * direction.X + fix * direction.Y);
        }

        private static BubbleFix CosRaw(int raw)
        {
            raw = ((raw < 0) ? (-raw) : raw);
            int num = raw & 0x3FF;
            raw >>= 10;
            if (num == 0)
            {
                return CosRawLookup(raw);
            }

            BubbleFix fix = CosRawLookup(raw);
            BubbleFix fix2 = CosRawLookup(raw + 1);
            return new BubbleFix((int)((long)fix.Raw * (long)(1024 - num) + (long)fix2.Raw * (long)num + 512 >> 10));
        }

        private static BubbleFix CosRawLookup(int raw)
        {
            raw %= 1440;
            if (raw < 360)
            {
                return _quarterSine[360 - raw];
            }

            if (raw < 720)
            {
                raw -= 360;
                return -_quarterSine[raw];
            }

            if (raw < 1080)
            {
                raw -= 720;
                return -_quarterSine[360 - raw];
            }

            raw -= 1080;
            return _quarterSine[raw];
        }

        public static BubbleFix Tan(BubbleFix degrees)
        {
            return Sin(degrees) / Cos(degrees);
        }

        public static BubbleFix Asin(BubbleFix value)
        {
            return Atan2(value, Sqrt((1 + value) * (1 - value)));
        }

        public static BubbleFix Acos(BubbleFix value)
        {
            return Atan2(Sqrt((1 + value) * (1 - value)), value);
        }

        public static BubbleFix Atan(BubbleFix value)
        {
            return Atan2(value, 1);
        }

        public static BubbleFix Atan2(BubbleFix y, BubbleFix x)
        {
            if (x == 0 && y == 0)
            {
                return 0;
            }

            BubbleFix result = 0;
            if (x < 0)
            {
                BubbleFix fix;
                BubbleFix fix2;
                if (y < 0)
                {
                    fix = -y;
                    fix2 = x;
                    result = -90;
                }
                else if (y > 0)
                {
                    fix = y;
                    fix2 = -x;
                    result = 90;
                }
                else
                {
                    fix = x;
                    fix2 = y;
                    result = 180;
                }

                x = fix;
                y = fix2;
            }

            for (int i = 0; i < 14; i++)
            {
                BubbleFix fix;
                BubbleFix fix2;
                if (y > 0)
                {
                    fix = x + (y >> i);
                    fix2 = y - (x >> i);
                    result += _cordicAngles[i];
                }
                else
                {
                    if (!(y < 0))
                    {
                        break;
                    }

                    fix = x - (y >> i);
                    fix2 = y + (x >> i);
                    result -= _cordicAngles[i];
                }

                x = fix;
                y = fix2;
            }

            return result;
        }

        public static BubbleFix Exp(BubbleFix value)
        {
            return Pow(E, value);
        }

        public static BubbleFix Pow(BubbleFix b, BubbleFix exp)
        {
            if (b == 1 || exp == 0)
            {
                return 1;
            }

            int num;
            BubbleFix result;
            if ((exp.Raw & 0xFFF) == 0)
            {
                num = exp.Raw + 2048 >> 12;
                BubbleFix fix;
                int num2;
                if (num < 0)
                {
                    fix = 1 / b;
                    num2 = -num;
                }
                else
                {
                    fix = b;
                    num2 = num;
                }

                result = 1;
                while (num2 > 0)
                {
                    if (((uint)num2 & (true ? 1u : 0u)) != 0)
                    {
                        result *= fix;
                    }

                    fix *= fix;
                    num2 >>= 1;
                }

                return result;
            }

            exp *= Log(b, 2);
            b = 2;
            num = exp.Raw + 2048 >> 12;
            result = ((num < 0) ? (BubbleFix.One >> -num) : (BubbleFix.One << num));
            long num3 = (exp.Raw - (num << 12)) * _ln2Const.Raw + 2048 >> 12;
            if (num3 == 0L)
            {
                return result;
            }

            long num4 = num3;
            long num5 = num3;
            for (int i = 2; i < _invFactConsts.Length; i++)
            {
                if (num5 == 0L)
                {
                    break;
                }

                num5 *= num3;
                num5 += 2147483648u;
                num5 >>= 32;
                long num6 = num5 * _invFactConsts[i].Raw;
                num6 += 2147483648u;
                num6 >>= 32;
                num4 += num6;
            }

            return new BubbleFix((int)((result.Raw * num4 + 2147483648u >> 32) + result.Raw));
        }

        public static BubbleFix Log(BubbleFix value)
        {
            return Log2(value) * _ln2;
        }

        public static BubbleFix Log(BubbleFix value, BubbleFix b)
        {
            if (b == 2)
            {
                return Log2(value);
            }

            if (b == E)
            {
                return Log(value);
            }

            if (b == 10)
            {
                return Log10(value);
            }

            return Log2(value) / Log2(b);
        }

        public static BubbleFix Log10(BubbleFix value)
        {
            return Log2(value) * _log10_2;
        }

        private static BubbleFix Log2(BubbleFix value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException("value", "Value must be positive.");
            }

            uint num = (uint)value.Raw;
            uint num2 = 2048u;
            uint num3 = 0u;
            while (num < 4096)
            {
                num <<= 1;
                num3 -= 4096;
            }

            while (num >= 8192)
            {
                num >>= 1;
                num3 += 4096;
            }

            ulong num4 = num;
            for (int i = 0; i < 12; i++)
            {
                num4 = num4 * num4 >> 12;
                if (num4 >= 8192)
                {
                    num4 >>= 1;
                    num3 += num2;
                }

                num2 >>= 1;
            }

            return new BubbleFix((int)num3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix Radians(BubbleFix degrees)
        {
            return degrees * Deg2Rad;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix ToRadians(this BubbleFix degrees)
        {
            return degrees * Deg2Rad;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix Degrees(BubbleFix radians)
        {
            return radians * Rad2Deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix ToDegrees(this BubbleFix radians)
        {
            return radians * Rad2Deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix Angle(this BubbleFixVec2 v)
        {
            return Atan2(v.Y, v.X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix CombatZoneAngle(this BubbleFixVec2 v)
        {
            return Atan2(-v.Y, v.X) + 90;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFixVec2 CombatZoneDirection(this BubbleFix combatZoneAngle)
        {
            combatZoneAngle += (BubbleFix)90;
            return new BubbleFixVec2(-Cos(combatZoneAngle), Sin(combatZoneAngle));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix UnityAngle(this BubbleFixVec2 v)
        {
            return Atan2(v.X, v.Y);
        }

        public static BubbleFixVec2 Polar(BubbleFix radians)
        {
            return new BubbleFixVec2(Cos(Degrees(radians)), Sin(Degrees(radians)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix Cross(BubbleFixVec2 a, BubbleFixVec2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix Dot(BubbleFixVec2 a, BubbleFixVec2 b)
        {
            return a.Dot(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BubbleFix Square(BubbleFix a)
        {
            return a * a;
        }

        public static long GetRawMultiply(BubbleFix lhs, BubbleFix rhs)
        {
            return (long)lhs.Raw * (long)rhs.Raw + 2048 >> 12;
        }

        public static long GetRawDotProduct(BubbleFixVec2 a, BubbleFixVec2 b)
        {
            return GetRawMultiply(a.X, b.X) + GetRawMultiply(a.Y, b.Y);
        }

        public static long DivideRaw(long lhs, long rhs)
        {
            return (lhs << 13) / rhs + 1 >> 1;
        }

        public static BubbleFix Clamp(BubbleFix value, BubbleFix min, BubbleFix max)
        {
            if (value > max)
            {
                return max;
            }

            if (value < min)
            {
                return min;
            }

            return value;
        }

        public static BubbleFix Lerp(BubbleFix a, BubbleFix b, BubbleFix t)
        {
            return a * (BubbleFix.One - t) + b * t;
        }

        public static BubbleFix InverseLerp(BubbleFix a, BubbleFix b, BubbleFix value)
        {
            return Clamp((value - a) / (b - a), BubbleFix.Zero, BubbleFix.One);
        }

        public static BubbleFixVec2 Mat2Mul(BubbleFixVec2 matCol0, BubbleFixVec2 matCol1, BubbleFixVec2 vector)
        {
            return new BubbleFixVec2(matCol0.X * vector.X + matCol0.Y * vector.Y, matCol1.X * vector.X + matCol1.Y * vector.Y);
        }

        public static BubbleFixVec2 Abs(BubbleFixVec2 value)
        {
            return new BubbleFixVec2(Abs(value.X), Abs(value.Y));
        }

        public static BubbleFixVec3 Abs(BubbleFixVec3 value)
        {
            return new BubbleFixVec3(Abs(value.X), Abs(value.Y), Abs(value.Z));
        }

        public static BubbleFixVec2 Min(BubbleFixVec2 v1, BubbleFixVec2 v2)
        {
            return new BubbleFixVec2(Min(v1.X, v2.X), Min(v1.Y, v2.Y));
        }

        public static BubbleFixVec2 Max(BubbleFixVec2 v1, BubbleFixVec2 v2)
        {
            return new BubbleFixVec2(Max(v1.X, v2.X), Max(v1.Y, v2.Y));
        }

        public static BubbleFixVec3 Min(BubbleFixVec3 v1, BubbleFixVec3 v2)
        {
            return new BubbleFixVec3(Min(v1.X, v2.X), Min(v1.Y, v2.Y), Min(v1.Z, v2.Z));
        }

        public static BubbleFixVec3 Max(BubbleFixVec3 v1, BubbleFixVec3 v2)
        {
            return new BubbleFixVec3(Max(v1.X, v2.X), Max(v1.Y, v2.Y), Max(v1.Z, v2.Z));
        }
    }

	public struct BubbleFix
	{
		internal const int FRACTIONAL_BITS = 12;

		internal const int INTEGER_BITS = 20;

		internal const int FRACTION_MASK = 4095;

		internal const int INTEGER_MASK = -4096;

		internal const int FRACTION_RANGE = 4096;

		internal const int MIN_INTEGER = -524288;

		internal const int MAX_INTEGER = 524287;

		public static readonly BubbleFix Zero;

		public static readonly BubbleFix One;

		public static readonly BubbleFix Half;

		public static readonly BubbleFix Quarter;

		public static readonly BubbleFix MinValue;

		public static readonly BubbleFix MaxValue;

		public static readonly BubbleFix Epsilon;

		private int _raw;

		public static int FractionalBits => 12;

		public static int IntegerBits => 20;

		public static int FractionMask => 4095;

		public static int IntegerMask => -4096;

		public static int FractionRange => 4096;

		public static int MinInteger => -524288;

		public static int MaxInteger => 524287;

		public int Raw
		{
			get
			{
				return _raw;
			}
			set
			{
				_raw = value;
			}
		}

		static BubbleFix()
		{
			Zero = new BubbleFix(0);
			One = new BubbleFix(4096);
			Half = new BubbleFix(0.5f);
			Quarter = new BubbleFix(0.25f);
			MinValue = new BubbleFix(int.MinValue);
			MaxValue = new BubbleFix(int.MaxValue);
			Epsilon = new BubbleFix(1);
		}

		public static BubbleFix Ratio(int numerator, int denominator)
		{
			return new BubbleFix(((long)numerator << 13) / denominator + 1 >> 1);
		}

		public static explicit operator double(BubbleFix value)
		{
			return (double)(value._raw >> 12) + (double)(value._raw & 0xFFF) / 4096.0;
		}

		public static explicit operator float(BubbleFix value)
		{
			return (float)(double)value;
		}

		public static explicit operator int(BubbleFix value)
		{
			if (value._raw > 0)
			{
				return value._raw >> 12;
			}

			return value._raw + 4095 >> 12;
		}

		public static implicit operator BubbleFix(int value)
		{
			if (value > MaxInteger || value < MinInteger)
			{
				value = ((value > MaxInteger) ? MaxInteger : MinInteger);
				//Log.Info(LogGroups.Engine, "Clamping BubbleFix value created outside of valid ranges. Expect problems");
			}

			return new BubbleFix(value << 12);
		}

		public static bool operator ==(BubbleFix lhs, BubbleFix rhs)
		{
			return lhs._raw == rhs._raw;
		}

		public static bool operator !=(BubbleFix lhs, BubbleFix rhs)
		{
			return lhs._raw != rhs._raw;
		}

		public static bool operator >(BubbleFix lhs, BubbleFix rhs)
		{
			return lhs._raw > rhs._raw;
		}

		public static bool operator >=(BubbleFix lhs, BubbleFix rhs)
		{
			return lhs._raw >= rhs._raw;
		}

		public static bool operator <(BubbleFix lhs, BubbleFix rhs)
		{
			return lhs._raw < rhs._raw;
		}

		public static bool operator <=(BubbleFix lhs, BubbleFix rhs)
		{
			return lhs._raw <= rhs._raw;
		}

		public static BubbleFix operator +(BubbleFix value)
		{
			return value;
		}

		public static BubbleFix operator -(BubbleFix value)
		{
			return new BubbleFix(-value._raw);
		}

		public static BubbleFix operator +(BubbleFix lhs, BubbleFix rhs)
		{
			return new BubbleFix(lhs._raw + rhs._raw);
		}

		public static BubbleFix operator -(BubbleFix lhs, BubbleFix rhs)
		{
			return new BubbleFix(lhs._raw - rhs._raw);
		}

		public static BubbleFix operator *(BubbleFix lhs, BubbleFix rhs)
		{
			return new BubbleFix((long)lhs._raw * (long)rhs._raw + 2048 >> 12);
		}

		public static BubbleFix operator /(BubbleFix lhs, BubbleFix rhs)
		{
			return new BubbleFix(((long)lhs._raw << 13) / rhs._raw + 1 >> 1);
		}

		public static BubbleFix operator %(BubbleFix lhs, BubbleFix rhs)
		{
			return new BubbleFix(lhs.Raw % rhs.Raw);
		}

		public static BubbleFix operator <<(BubbleFix lhs, int rhs)
		{
			return new BubbleFix(lhs.Raw << rhs);
		}

		public static BubbleFix operator >>(BubbleFix lhs, int rhs)
		{
			return new BubbleFix(lhs.Raw >> rhs);
		}

		public static bool Approximately(BubbleFix fixA, BubbleFix fixB, int epsilonRaw = 2)
		{
			return Math.Abs(fixA.Raw - fixB.Raw) <= epsilonRaw;
		}

		public BubbleFix(int raw)
		{
			_raw = raw;
		}

		private BubbleFix(long raw)
		{
			if (raw > int.MaxValue || raw < int.MinValue)
			{
				raw = ((raw > int.MaxValue) ? int.MaxValue : int.MinValue);
				//Log.Info(LogGroups.Engine, "Clamping BubbleFix value created outside of valid ranges. Expect problems");
			}

			_raw = (int)raw;
		}

		public BubbleFix(float input)
		{
			CalculateFraction(input, out var numerator, out var denominator, 4096L);
			_raw = (int)((numerator << 13) / denominator + 1 >> 1);
		}
        public unsafe static void CalculateFraction(float f, out long numerator, out long denominator, long MaximumDenominator = 4096L)
        {
            try
            {
                byte* ip = stackalloc byte[24];
                Span<long> span = new(ip, 3);
                span[0] = 0L;
                span[1] = 1L;
                span[2] = 0L;

                byte* ip2 = stackalloc byte[24];
                Span<long> span2 = new(ip2, 3);
                span2[0] = 1L;
                span2[1] = 0L;
                span2[2] = 0L;

                long num = 1L;
                int num2 = 0;
                if (float.IsNaN(f))
                {
                    //Log.Error(LogGroups.Engine, "[CalculateFraction] NaN float value passed. Maybe there's a float or double divide-by-zero in earlier code?", LogOptions.None, "CalculateFraction", "C:\\jenkins\\workspace\\Shoe_artemis_staging-next-season\\ArtemisClientUnity\\Assets\\Scripts\\WorldEngine\\FixedPointy\\Fraction.cs", 21);
                    denominator = 1L;
                    numerator = long.MaxValue;
                }
                else if (MaximumDenominator <= 1L)
                {
                    denominator = 1L;
                    numerator = (long)f;
                }
                else
                {
                    if (f < 0f)
                    {
                        num2 = 1;
                        f = -f;
                    }
                    while ((double)f != Math.Floor((double)f))
                    {
                        num <<= 1;
                        f *= 2f;
                    }
                    long num3 = (long)f;
                    for (int i = 0; i < 64; i++)
                    {
                        long num4 = (num != 0L) ? (num3 / num) : 0L;
                        if ((i != 0 && num4 == 0L) || num == 0L)
                        {
                            break;
                        }
                        long num5 = num3;
                        num3 = num;
                        num = num5 % num;
                        num5 = num4;
                        if (span2[1] * num4 + span2[0] >= MaximumDenominator)
                        {
                            num5 = (MaximumDenominator - span2[0]) / span2[1];
                            if (num5 * 2L < num4 && span2[1] < MaximumDenominator)
                            {
                                break;
                            }
                            i = 65;
                        }
                        span[2] = num5 * span[1] + span[0];
                        span[0] = span[1];
                        span[1] = span[2];
                        span2[2] = num5 * span2[1] + span2[0];
                        span2[0] = span2[1];
                        span2[1] = span2[2];
                    }
                    denominator = span2[1];
                    numerator = ((num2 != 0) ? (-(span[1])) : (span[1]));
                    if (denominator == 0L)
                    {
                        denominator = 1L;
                        numerator = 0L;
                    }
                }
            }
            catch (Exception)
            {
                //Log.Error(LogGroups.Engine, string.Format("[CalculateFraction] Exception using input float value: [DDI] {0}\n{1}", f, arg), LogOptions.None, "CalculateFraction", "C:\\jenkins\\workspace\\Shoe_artemis_staging-next-season\\ArtemisClientUnity\\Assets\\Scripts\\WorldEngine\\FixedPointy\\Fraction.cs", 76);
                denominator = 1L;
                numerator = long.MaxValue;
            }
        }

        public override bool Equals(object obj)
        {
			if (obj is BubbleFix)
			{
				return (BubbleFix)obj == this;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return GetConsistentHashCode();
		}

		public int GetConsistentHashCode()
		{
			return ShinyShoe.ConsistentHash.GetHash(Raw);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_raw < 0)
			{
				stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);
			}

			int num = (int)this;
			stringBuilder.Append(((num < 0) ? (-num) : num).ToString());
			ulong num2 = (ulong)(int)((uint)_raw & 0xFFFu);
			if (num2 == 0L)
			{
				return stringBuilder.ToString();
			}

			num2 = ((_raw < 0) ? (4096 - num2) : num2);
			num2 *= 1000000;
			num2 += 2048;
			num2 >>= 12;
			stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
			stringBuilder.Append(num2.ToString("D6").TrimEnd('0'));
			return stringBuilder.ToString();
		}
	}
	public struct BubbleFixConst
	{
		private long _raw;

		public long Raw => _raw;

		public static explicit operator double(BubbleFixConst f)
		{
			return (double)(f._raw >> 32) + (double)(uint)f._raw / 4294967296.0;
		}

		public static implicit operator BubbleFixConst(double value)
		{
			if (value < -2147483648.0 || value >= 2147483648.0)
			{
				throw new OverflowException();
			}

			double num = Math.Floor(value);
			return new BubbleFixConst(((long)num << 32) + (long)((value - num) * 4294967296.0 + 0.5));
		}

		public static implicit operator BubbleFix(BubbleFixConst value)
		{
			return new BubbleFix((int)(value.Raw + 524288 >> 20));
		}

		public static explicit operator int(BubbleFixConst value)
		{
			if (value._raw > 0)
			{
				return (int)(value._raw >> 32);
			}

			return (int)(value._raw + uint.MaxValue >> 32);
		}

		public static implicit operator BubbleFixConst(int value)
		{
			return new BubbleFixConst((long)value << 32);
		}

		public static bool operator ==(BubbleFixConst lhs, BubbleFixConst rhs)
		{
			return lhs._raw == rhs._raw;
		}

		public static bool operator !=(BubbleFixConst lhs, BubbleFixConst rhs)
		{
			return lhs._raw != rhs._raw;
		}

		public static bool operator >(BubbleFixConst lhs, BubbleFixConst rhs)
		{
			return lhs._raw > rhs._raw;
		}

		public static bool operator >=(BubbleFixConst lhs, BubbleFixConst rhs)
		{
			return lhs._raw >= rhs._raw;
		}

		public static bool operator <(BubbleFixConst lhs, BubbleFixConst rhs)
		{
			return lhs._raw < rhs._raw;
		}

		public static bool operator <=(BubbleFixConst lhs, BubbleFixConst rhs)
		{
			return lhs._raw <= rhs._raw;
		}

		public static BubbleFixConst operator +(BubbleFixConst value)
		{
			return value;
		}

		public static BubbleFixConst operator -(BubbleFixConst value)
		{
			return new BubbleFixConst(-value._raw);
		}

		public BubbleFixConst(long raw)
		{
			_raw = raw;
		}

		public override bool Equals(object obj)
		{
			if (obj is BubbleFixConst)
			{
				return (BubbleFixConst)obj == this;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return ShinyShoe.ConsistentHash.GetHash(Raw);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_raw < 0)
			{
				stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);
			}

			long num = (int)this;
			stringBuilder.Append(((num < 0) ? (-num) : num).ToString());
			ulong num2 = (ulong)(_raw & 0xFFFFFFFFu);
			if (num2 == 0L)
			{
				return stringBuilder.ToString();
			}

			num2 = ((_raw < 0) ? (4294967296L - num2) : num2);
			num2 *= 1000000000;
			num2 += 2147483648u;
			num2 >>= 32;
			stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
			stringBuilder.Append(num2.ToString("D9").TrimEnd('0'));
			return stringBuilder.ToString();
		}
	}
	public struct BubbleFixVec2 : IEquatable<BubbleFixVec2>
	{
		public static readonly BubbleFixVec2 Zero = default(BubbleFixVec2);

		public static readonly BubbleFixVec2 One = new BubbleFixVec2(1, 1);

		public static readonly BubbleFixVec2 UnitX = new BubbleFixVec2(1, 0);

		public static readonly BubbleFixVec2 UnitY = new BubbleFixVec2(0, 1);

		private BubbleFix _x;

		private BubbleFix _y;

		public BubbleFix X
		{
			get
			{
				return _x;
			}
			set
			{
				_x = value;
			}
		}

		public BubbleFix Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
			}
		}

		public static BubbleFixVec2 operator +(BubbleFixVec2 rhs)
		{
			return rhs;
		}

		public static BubbleFixVec2 operator -(BubbleFixVec2 rhs)
		{
			return new BubbleFixVec2(-rhs._x, -rhs._y);
		}

		public static BubbleFixVec2 operator +(BubbleFixVec2 lhs, BubbleFixVec2 rhs)
		{
			return new BubbleFixVec2(lhs._x + rhs._x, lhs._y + rhs._y);
		}

		public static BubbleFixVec2 operator -(BubbleFixVec2 lhs, BubbleFixVec2 rhs)
		{
			return new BubbleFixVec2(lhs._x - rhs._x, lhs._y - rhs._y);
		}

		public static BubbleFixVec2 operator +(BubbleFixVec2 lhs, BubbleFix rhs)
		{
			return lhs.ScalarAdd(rhs);
		}

		public static BubbleFixVec2 operator +(BubbleFix lhs, BubbleFixVec2 rhs)
		{
			return rhs.ScalarAdd(lhs);
		}

		public static BubbleFixVec2 operator -(BubbleFixVec2 lhs, BubbleFix rhs)
		{
			return new BubbleFixVec2(lhs._x - rhs, lhs._y - rhs);
		}

		public static BubbleFixVec2 operator *(BubbleFixVec2 lhs, BubbleFix rhs)
		{
			return lhs.ScalarMultiply(rhs);
		}

		public static BubbleFixVec2 operator *(BubbleFix lhs, BubbleFixVec2 rhs)
		{
			return rhs.ScalarMultiply(lhs);
		}

		public static BubbleFixVec2 operator /(BubbleFixVec2 lhs, BubbleFix rhs)
		{
			return new BubbleFixVec2(lhs._x / rhs, lhs._y / rhs);
		}

		public static bool operator ==(BubbleFixVec2 lhs, BubbleFixVec2 rhs)
		{
			if (lhs.X == rhs.X)
			{
				return lhs.Y == rhs.Y;
			}

			return false;
		}

		public static bool operator !=(BubbleFixVec2 lhs, BubbleFixVec2 rhs)
		{
			if (!(lhs.X != rhs.X))
			{
				return lhs.Y != rhs.Y;
			}

			return true;
		}

		public BubbleFixVec2(BubbleFix x, BubbleFix y)
		{
			_x = x;
			_y = y;
		}

		public BubbleFix Dot(BubbleFixVec2 rhs)
		{
			return _x * rhs._x + _y * rhs._y;
		}

		public BubbleFix Cross(BubbleFixVec2 rhs)
		{
			return _x * rhs._y - _y * rhs._x;
		}

		private BubbleFixVec2 ScalarAdd(BubbleFix value)
		{
			return new BubbleFixVec2(_x + value, _y + value);
		}

		private BubbleFixVec2 ScalarMultiply(BubbleFix value)
		{
			return new BubbleFixVec2(_x * value, _y * value);
		}

		public BubbleFix GetMagnitude()
		{
			ulong num = (ulong)((long)_x.Raw * (long)_x.Raw + (long)_y.Raw * (long)_y.Raw);
			if (num == 0L)
			{
				return BubbleFix.Zero;
			}

			return new BubbleFix((int)(BubbleFixMath.SqrtULong(num << 2) + 1) >> 1);
		}

		public BubbleFix GetSqrMagnitude()
		{
			return _x * _x + _y * _y;
		}

		public BubbleFixVec2 GetOrthoCW()
		{
			return new BubbleFixVec2(Y, -X);
		}

		public BubbleFixVec2 GetOrthoCCW()
		{
			return new BubbleFixVec2(-Y, X);
		}

		public BubbleFixVec2 Normalize()
		{
			if (_x == 0 && _y == 0)
			{
				return Zero;
			}

			BubbleFix magnitude = GetMagnitude();
			return new BubbleFixVec2(_x / magnitude, _y / magnitude);
		}

		public bool Equals(BubbleFixVec2 other)
		{
			if (X == other.X)
			{
				return Y == other.Y;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is BubbleFixVec2)
			{
				return Equals((BubbleFixVec2)obj);
			}

			return false;
		}

		public override string ToString()
		{
			return $"({_x}, {_y})";
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
	public struct BubbleFixVec3 : IEquatable<BubbleFixVec3>
	{
		public static readonly BubbleFixVec3 Zero = default(BubbleFixVec3);

		public static readonly BubbleFixVec3 One = new BubbleFixVec3(1, 1, 1);

		public static readonly BubbleFixVec3 UnitX = new BubbleFixVec3(1, 0, 0);

		public static readonly BubbleFixVec3 UnitY = new BubbleFixVec3(0, 1, 0);

		public static readonly BubbleFixVec3 UnitZ = new BubbleFixVec3(0, 0, 1);

		private BubbleFix _x;

		private BubbleFix _y;

		private BubbleFix _z;

		public BubbleFix X
		{
			get
			{
				return _x;
			}
			set
			{
				_x = value;
			}
		}

		public BubbleFix Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
			}
		}

		public BubbleFix Z
		{
			get
			{
				return _z;
			}
			set
			{
				_z = value;
			}
		}

		public BubbleFixVec2 XZ
		{
			get
			{
				return new BubbleFixVec2(_x, _z);
			}
			set
			{
				_x = value.X;
				_z = value.Y;
			}
		}

		public static implicit operator BubbleFixVec3(BubbleFixVec2 value)
		{
			return new BubbleFixVec3(value.X, value.Y, 0);
		}

		public static BubbleFixVec3 operator +(BubbleFixVec3 rhs)
		{
			return rhs;
		}

		public static BubbleFixVec3 operator -(BubbleFixVec3 rhs)
		{
			return new BubbleFixVec3(-rhs._x, -rhs._y, -rhs._z);
		}

		public static BubbleFixVec3 operator +(BubbleFixVec3 lhs, BubbleFixVec3 rhs)
		{
			return new BubbleFixVec3(lhs._x + rhs._x, lhs._y + rhs._y, lhs._z + rhs._z);
		}

		public static BubbleFixVec3 operator -(BubbleFixVec3 lhs, BubbleFixVec3 rhs)
		{
			return new BubbleFixVec3(lhs._x - rhs._x, lhs._y - rhs._y, lhs._z - rhs._z);
		}

		public static BubbleFixVec3 operator +(BubbleFixVec3 lhs, BubbleFix rhs)
		{
			return lhs.ScalarAdd(rhs);
		}

		public static BubbleFixVec3 operator +(BubbleFix lhs, BubbleFixVec3 rhs)
		{
			return rhs.ScalarAdd(lhs);
		}

		public static BubbleFixVec3 operator -(BubbleFixVec3 lhs, BubbleFix rhs)
		{
			return new BubbleFixVec3(lhs._x - rhs, lhs._y - rhs, lhs._z - rhs);
		}

		public static BubbleFixVec3 operator *(BubbleFixVec3 lhs, BubbleFix rhs)
		{
			return lhs.ScalarMultiply(rhs);
		}

		public static BubbleFixVec3 operator *(BubbleFix lhs, BubbleFixVec3 rhs)
		{
			return rhs.ScalarMultiply(lhs);
		}

		public static BubbleFixVec3 operator /(BubbleFixVec3 lhs, BubbleFix rhs)
		{
			return new BubbleFixVec3(lhs._x / rhs, lhs._y / rhs, lhs._z / rhs);
		}

		public BubbleFixVec3(BubbleFix x, BubbleFix y, BubbleFix z)
		{
			_x = x;
			_y = y;
			_z = z;
		}

		public void FromRawValues(int x, int y, int z)
		{
			_x.Raw = x;
			_y.Raw = y;
			_z.Raw = z;
		}

		public BubbleFix Dot(BubbleFixVec3 rhs)
		{
			return _x * rhs._x + _y * rhs._y + _z * rhs._z;
		}

		public BubbleFixVec3 Cross(BubbleFixVec3 rhs)
		{
			return new BubbleFixVec3(_y * rhs._z - _z * rhs._y, _z * rhs._x - _x * rhs._z, _x * rhs._y - _y * rhs._x);
		}

		private BubbleFixVec3 ScalarAdd(BubbleFix value)
		{
			return new BubbleFixVec3(_x + value, _y + value, _z + value);
		}

		private BubbleFixVec3 ScalarMultiply(BubbleFix value)
		{
			return new BubbleFixVec3(_x * value, _y * value, _z * value);
		}

		public BubbleFix GetMagnitude()
		{
			ulong num = (ulong)((long)_x.Raw * (long)_x.Raw + (long)_y.Raw * (long)_y.Raw + (long)_z.Raw * (long)_z.Raw);
			if (num == 0L)
			{
				return BubbleFix.Zero;
			}

			return new BubbleFix((int)(BubbleFixMath.SqrtULong(num << 2) + 1) >> 1);
		}

		public BubbleFixVec3 Normalize()
		{
			if (_x == 0 && _y == 0 && _z == 0)
			{
				return Zero;
			}

			BubbleFix magnitude = GetMagnitude();
			return new BubbleFixVec3(_x / magnitude, _y / magnitude, _z / magnitude);
		}

		public BubbleFixVec3 ComponentMultiply(BubbleFixVec3 rhs)
		{
			return new BubbleFixVec3(_x * rhs.X, _y * rhs.Y, _z * rhs.Z);
		}

		public override string ToString()
		{
			return $"({_x}, {_y}, {_z})";
		}

		public static bool operator ==(BubbleFixVec3 left, BubbleFixVec3 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BubbleFixVec3 left, BubbleFixVec3 right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj is BubbleFixVec3)
			{
				BubbleFixVec3 other = (BubbleFixVec3)obj;
				return Equals(other);
			}

			return false;
		}

		public bool Equals(BubbleFixVec3 other)
		{
			if (EqualityComparer<BubbleFix>.Default.Equals(X, other.X) && EqualityComparer<BubbleFix>.Default.Equals(Y, other.Y))
			{
				return EqualityComparer<BubbleFix>.Default.Equals(Z, other.Z);
			}

			return false;
		}

		public override int GetHashCode()
		{
            unchecked {
                int core = -307843816 * -1521134295;
                return ((core + X.GetConsistentHashCode()) * -1521134295 + Y.GetConsistentHashCode()) * -1521134295 + Z.GetConsistentHashCode();
            };
		}
	}

}
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).