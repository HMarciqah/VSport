using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSport
{
    public partial class MainWindow : Window
    {
        private int egyenleg = 0;
        private string jatekosNev = "";
        private bool meccsKozben = false;
        private Random rnd = new Random();



        // csapatadatbazisok
        private string[] cspatok = {
            // Angol bajnokság
            "Man. City", "Arsenal", "Liverpool", "Man. United", "Chelsea", "Tottenham", "Newcastle United",

            // Spanyol bajnokság
            "Real Madrid", "Barcelona", "Atletico Madrid", "Sevilla", "Valencia",

            // Olasz bajnokság
            "Juventus", "AC Milan", "Inter", "Napoli", "AS Roma", "Lazio",

            // Német bajnokság
            "Bayern München", "Borussia Dortmund", "Bayer Leverkusen", "RB Leipzig", };



        private double aktualisOdds1;
        private double aktualisOddsX;
        private double aktualisOdds2;
        private double aktualisOddsOver;
        private double aktualisOddsSerules;
        private double aktualisOddsBefuto;
        public MainWindow()
        {
            InitializeComponent();
            // az indulaskor a jatekmezo ures legyen
            StatuszSzoveg.Text = "Kérjük, jelentkezzen be...";
        }


        // bejelentkezes es feltoltes
        private async void Belepes_Click(object sender, RoutedEventArgs e)
        {
            string beirtNev = LoginNevInput.Text.Trim();




            // hibakezeles
            if (string.IsNullOrEmpty(beirtNev) || !int.TryParse(LoginPenzInput.Text, out int kezdoPenz) || kezdoPenz <= 0)
            {
                // razkodo animacio a hibas adatnal
                var razkodas = new DoubleAnimation(-8, 8, TimeSpan.FromMilliseconds(50)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) };
                LoginPanelTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, razkodas);
                return;
            }



            // ertekek beallitasa
            jatekosNev = beirtNev;
            egyenleg = kezdoPenz;


            FelhasznaloNevSzoveg.Text = $"Üdvözöljük, {jatekosNev}!";
            EgyenlegSzoveg.Text = $"Egyenleg: {egyenleg:N0} Ft";
            TetInput.Text = (egyenleg / 10).ToString(); // alapbol a penz 10°%-at veszi


            // Overlay panel eltuntetese
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, ev) => LoginOverlay.Visibility = Visibility.Collapsed;
            LoginOverlay.BeginAnimation(OpacityProperty, fadeOut);


            // jatek inditasi folyamat
            UjMekozesesGeneralasa();
            EsemenyLog.Items.Insert(0, $"Üdvözlünk a vSport Pro rendszerében, {jatekosNev}!"); // beszur egy uj elemet az EsemenyLog lista elejere, ezzel a legfrissebb esemény mindig felul jelenik meg
            EsemenyLog.Items.Insert(0, $"Sikeres egyenleg feltöltés: {egyenleg:N0} Ft.");


            // kartyak latvanyos becsuszasa
            await AnimaciokInditasa(); // a program megvarja, amig az AnimaciokInditasa() nevu aszinkron metodus lefut, mielott tovabblepne a kovetkezo sorra
        }



        private async Task AnimaciokInditasa() // ez azt jelenti, hogy a metodus csak az adott osztalybol erheto el, kivulrol nem lehet meghivni
        {
            foreach (var item in FogadasPanel.Children) // egy gyerek elemeket tartalmazo kollekciot ad vissza egy kontenerhez
            {
                if (item is Border b)
                {
                    // ez a kod egy UI elemet 400 ms alatt fokozatosan felnagyit es felcsusztat animacioval jelenit meg, kis kesest adva a sorozatos effekthez
                    b.Opacity = 0;
                    b.RenderTransform = new System.Windows.Media.TranslateTransform(0, 30);
                    var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                    var slide = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(400)) { EasingFunction = new CircleEase() };
                    b.BeginAnimation(OpacityProperty, fade);
                    b.RenderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slide);
                    await Task.Delay(80);
                }
            }
        }



        // jatek logika
        private void UjMekozesesGeneralasa()
        {
            int hazaiIndex = rnd.Next(cspatok.Length);
            int vendegIndex;
            do { vendegIndex = rnd.Next(cspatok.Length); } while (hazaiIndex == vendegIndex);
            HazaiCsapatNev.Text = cspatok[hazaiIndex];
            VendegCsapatNev.Text = cspatok[vendegIndex];
            aktualisOdds1 = Math.Round(rnd.NextDouble() * 2.5 + 1.2, 2);
            aktualisOddsX = Math.Round(rnd.NextDouble() * 1.5 + 2.8, 2);
            aktualisOdds2 = Math.Round(rnd.NextDouble() * 2.5 + 1.3, 2);
            aktualisOddsOver = Math.Round(rnd.NextDouble() * 1.2 + 1.4, 2);
            aktualisOddsSerules = Math.Round(rnd.NextDouble() * 3.0 + 3.0, 2);
            aktualisOddsBefuto = Math.Round(rnd.NextDouble() * 15.0 + 8.0, 2);
            Szorzo1.Text = aktualisOdds1.ToString("0.00");
            SzorzoX.Text = aktualisOddsX.ToString("0.00");
            Szorzo2.Text = aktualisOdds2.ToString("0.00");
            SzorzoOver.Text = aktualisOddsOver.ToString("0.00");
            SzorzoSerules.Text = aktualisOddsSerules.ToString("0.00");
            SzorzoBefuto.Text = aktualisOddsBefuto.ToString("0.00");
            EredmenySzoveg.Text = "0 : 0";
            StatuszSzoveg.Text = "Várakozás a fogadásra...";
        }



        private void HibasTetAnimacio()
        {
            // gyorsan jobbra balra rázogat 3-szor, miközben egy figyelmezteto uzenetet ir ki a lista elejere
            var razkodas = new DoubleAnimation(-5, 5, TimeSpan.FromMilliseconds(60)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) };
            TetTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, razkodas);
            EsemenyLog.Items.Insert(0, "⚠️ Érvénytelen tét vagy nincs elég egyenleg!");
        }


        private async void JatekInditasa_Click(object sender, RoutedEventArgs e)
        {
            if (meccsKozben) return;
            if (!int.TryParse(TetInput.Text, out int tet) || tet <= 0 || tet > egyenleg)
            {
                HibasTetAnimacio();
                return;
            }
            double összesOdds = 1.0;
            int tippekSzama = 0;
            if (Chk1.IsChecked == true) { összesOdds *= aktualisOdds1; tippekSzama++; }
            if (ChkX.IsChecked == true) { összesOdds *= aktualisOddsX; tippekSzama++; }
            if (Chk2.IsChecked == true) { összesOdds *= aktualisOdds2; tippekSzama++; }
            if (ChkOver.IsChecked == true) { összesOdds *= aktualisOddsOver; tippekSzama++; }
            if (ChkSerules.IsChecked == true) { összesOdds *= aktualisOddsSerules; tippekSzama++; }
            if (ChkBefuto.IsChecked == true) { összesOdds *= aktualisOddsBefuto; tippekSzama++; }
            if (tippekSzama == 0)
            {
                HibasTetAnimacio();
                EsemenyLog.Items.Insert(0, "⚠️ Válassz ki legalább egy eseményt!");
                return;
            }
            egyenleg -= tet;
            EgyenlegSzoveg.Text = $"Egyenleg: {egyenleg:N0} Ft";

            EsemenyLog.Items.Clear();
            EsemenyLog.Items.Insert(0, $"⚽ Mérkőzés elindítva! Tét: {tet} Ft | Várható: {tet * összesOdds:N0} Ft");

            await SzimulacioInditasa(tet, összesOdds);
        }


        private async Task SzimulacioInditasa(int tet, double összesOdds)
        {
            meccsKozben = true;
            int hazaiGol = 0; int vendegGol = 0;
            bool voltSerules = false; bool voltBefuto = false;
            for (int perc = 1; perc <= 90; perc += 3)
            {
                StatuszSzoveg.Text = $"Élő: {perc}. perc";
                await Task.Delay(200); // ez a sor egyszeruen 200 millimasodpercre megallitja a futast, mielott tovabb lepne a kod
                int esemenyEsely = rnd.Next(1, 100);
                if (esemenyEsely <= 6)
                {
                    if (rnd.Next(0, 2) == 0) hazaiGol++; else vendegGol++;
                    ((Storyboard)FindResource("GoalFlash")).Begin(EredmenyBorder); // megkeresi a "GoalFlash" nevu Storyboard eroforrast az XAML-ban, majd elinditja az EredmenyBorder elemre alkalmazva
                    EredmenySzoveg.Text = $"{hazaiGol} : {vendegGol}";
                    EsemenyLog.Items.Insert(0, $"🔥 {perc}. perc: GÓL! Eredmény: {hazaiGol}-{vendegGol}");
                }
                else if (esemenyEsely == 7 && !voltSerules)
                {
                    voltSerules = true;
                    EsemenyLog.Items.Insert(0, $"🚑 {perc}. perc: Szabálytalanság, hordágy jön a pályára.");
                }
                else if (esemenyEsely == 8 && !voltBefuto)
                {
                    voltBefuto = true;
                    EsemenyLog.Items.Insert(0, $"🏃‍♂️ {perc}. perc: Pályára befutó zavarja meg a meccset!");
                }
            }
            StatuszSzoveg.Text = "Mérkőzés véget ért (90')";
            Kiertekeles(tet, összesOdds, hazaiGol, vendegGol, voltSerules, voltBefuto);

            await Task.Delay(2000);
            UjMekozesesGeneralasa();
            EsemenyLog.Items.Insert(0, "🔄 Új mérkőzés elérhető a fogadáshoz!");
            meccsKozben = false;
        }



        private void Kiertekeles(int tet, double összesOdds, int hGol, int vGol, bool serules, bool befuto)
        {
            bool nyert = true;
            if (Chk1.IsChecked == true && hGol <= vGol) nyert = false;
            if (ChkX.IsChecked == true && hGol != vGol) nyert = false;
            if (Chk2.IsChecked == true && hGol >= vGol) nyert = false;
            if (ChkOver.IsChecked == true && (hGol + vGol) < 3) nyert = false;
            if (ChkSerules.IsChecked == true && !serules) nyert = false;
            if (ChkBefuto.IsChecked == true && !befuto) nyert = false;
            if (nyert)
            {
                int nyeremenyPenz = (int)(tet * összesOdds);
                egyenleg += nyeremenyPenz;
                EsemenyLog.Items.Insert(0, $"✅ NYERT SZELVÉNY! Nyereményed: {nyeremenyPenz:N0} Ft.");
            }
            else
            {
                EsemenyLog.Items.Insert(0, "❌ A szelvényed buktad.");
            }
            EgyenlegSzoveg.Text = $"Egyenleg: {egyenleg:N0} Ft";
            Chk1.IsChecked = ChkX.IsChecked = Chk2.IsChecked = false;
            ChkOver.IsChecked = ChkSerules.IsChecked = ChkBefuto.IsChecked = false;
        }
    }
}