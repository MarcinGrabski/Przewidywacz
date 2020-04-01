using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Przewidywacz
{
    public partial class OknoGlowne : Form
    {
        #region zmienne
        string zapytanie = "", podzapytanie = "", zapytanie2 = "";
        SqlCommand komenda = null, podkomenda = null, komenda2 = null;
        SqlDataReader czytnik = null, podczytnik = null, czytnik2 = null;
        SqlDataAdapter adapter = null, podadapter = null;

        public SqlConnection polaczenie = new SqlConnection("Data Source=JAVA\\SQLEXPRESS; Initial Catalog=sport; Integrated Security=True;MultipleActiveResultSets=true;");

        string zrodlo = "", line = "", data = "", gosp = "", gosc = "", kursGosp = "0.00", kursRemis = "0.00", kursGosc = "0.00", link = "", sezon = "";
        int spr = 0, start = 0, end = 0, runda = 0, wgosp = 0, wgosc = 0, w1gosp = 0, w1gosc = 0, w2gosp = 0, w2gosc = 0, czyWynik = 0, ileSpotkanBaza = 0, ileRekordow = 0;
        List<string> dane = new List<string>(); // skrócone źródło strony
        List<dynamic> adresy = new List<dynamic>(); // lista adresów lig
        List<dynamic> spotkania = new List<dynamic>(); // lista spotkań lig
        List<string> wyniki = new List<string>(); // obrobione wyniki spotkań
        List<string> wynikSz = new List<string>(); // obrobione dokładne wyniki spotkań
        List<string> przygotowaneDane = new List<string>(); // tylko nie powtarzające się wyniki
        List<string> DaneLigaSezon = new List<string>();
        List<string> daneOstateczneSpotkania = new List<string>();        
        List<string> listaSpotkania = new List<string>(); // skrócone źródło strony spotkań
        List<string> daneSpotkania = new List<string>(); // przerobione spotkania w csv
        SortedSet<string> DaneLigaGospSezon = new SortedSet<string>();

        #endregion

        private void PiłkaNożnaDowolnyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Visible = false;
            gbAdres.Visible = true;
            txtAdres.Text = "";
            txtAdres.Select();
        }

        private async void BtnPobierz_Click(object sender, EventArgs e)
        {
            sezon = "";
            WyczyscListy();
            zrodlo = await PobierzDane(txtAdres.Text);
            await SkrocZrodlo2(zrodlo);
            PobierzSezon();
            status.Text = "Pobieram wyniki ligi: " + txtLiga.Text + " z sezonu: " + sezon;
            await PobierzWyniki(txtLiga.Text);
            await IloscSpotkan(txtLiga.Text, sezon);
            PrzygotujDane();
            await DodajDaneDoBazy();
            gbAdres.Visible = false;
            richTextBox1.Visible = true;
            status.Text = "Gotowy";
        }

        public OknoGlowne()
        {
            InitializeComponent();
            PrzypiszAdresy();
            PrzypiszSpotkania();
        }

        private void PiłkaNożnaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TypowanieNizej35Wyzej15();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            status.Text = "Obliczenia...";
            await CzyszczenieTabel();
            await PobierzDaneLigaSezon();
            await PobierzDaneLigaGospSezon("gosp");
            await PobierzDaneLigaGospSezon("gosc");
            await TworzenieTabel();
            await PrzypiszMiejscaTabeli(0, "p_dom_tmp", "p_dom");
            await PrzypiszMiejscaTabeli(0, "p_wyjazd_tmp", "p_wyjazd");
            await PrzypiszMiejscaTabeli(0, "p_wszystko_tmp", "p_wszystko");

            await UsunSpotkania();
            await PrzygotujDaneSpotkania();
            await DodajSpotkaniaDoBazy();
            status.Text = "Gotowy";
        }

        #region InicjalizacjaDanych
        private void PrzypiszAdresy()
        {
            adresy.Add(new { liga = "Anglia 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/premier-league/results/" });
            adresy.Add(new { liga = "Anglia 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/championship/results/" });
            adresy.Add(new { liga = "Anglia 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/league-one/results/" });
            adresy.Add(new { liga = "Anglia 4", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/league-two/results/" });            
            adresy.Add(new { liga = "Brazylia 1", sezon = "2019", adres = "https://www.betexplorer.com/soccer/brazil/serie-a/results/" });
            adresy.Add(new { liga = "Brazylia 2", sezon = "2019", adres = "https://www.betexplorer.com/soccer/brazil/serie-b/results/" });
            adresy.Add(new { liga = "Bułgaria 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/bulgaria/parva-liga/results/" });
            adresy.Add(new { liga = "Czechy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/czech-republic/1-liga/results/" });
            adresy.Add(new { liga = "Czechy 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/czech-republic/division-2/results/" });
            adresy.Add(new { liga = "Dania 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/denmark/superliga/results/" });
            adresy.Add(new { liga = "Dania 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/denmark/1st-division/results/" });
            adresy.Add(new { liga = "Francja 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/france/ligue-1/results/" });
            adresy.Add(new { liga = "Francja 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/france/ligue-2/results/" });
            adresy.Add(new { liga = "Francja 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/france/national/results/" });
            adresy.Add(new { liga = "Hiszpania 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/spain/laliga/results/" });
            adresy.Add(new { liga = "Hiszpania 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/spain/laliga2/results/" });
            adresy.Add(new { liga = "Holandia 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/netherlands/eredivisie/results/" });
            adresy.Add(new { liga = "Holandia 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/netherlands/eerste-divisie/results/" });
            adresy.Add(new { liga = "Niemcy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/germany/bundesliga/results/" });
            adresy.Add(new { liga = "Niemcy 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/germany/2-bundesliga/results/" });
            adresy.Add(new { liga = "Niemcy 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/germany/3-liga/results/" });
            adresy.Add(new { liga = "Polska 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/poland/ekstraklasa/results/" });
            adresy.Add(new { liga = "Polska 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/poland/division-1/results/" });
            adresy.Add(new { liga = "Polska 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/poland/division-2/results/" });
            adresy.Add(new { liga = "Portugalia 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/portugal/primeira-liga/results/" });
            adresy.Add(new { liga = "Portugalia 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/portugal/ligapro/results/" });
            adresy.Add(new { liga = "Włochy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/italy/serie-a/results/" });
            adresy.Add(new { liga = "Włochy 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/italy/serie-b/results/" });
        }

        private void PrzypiszSpotkania()
        {
            spotkania.Add(new { liga = "Anglia 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/premier-league/fixtures/" });
            spotkania.Add(new { liga = "Anglia 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/championship/fixtures/" });
            spotkania.Add(new { liga = "Anglia 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/league-one/fixtures/" });
            spotkania.Add(new { liga = "Anglia 4", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/england/league-two/fixtures/" });
            spotkania.Add(new { liga = "Brazylia 1", sezon = "2019", adres = "https://www.betexplorer.com/soccer/brazil/serie-a/fixtures/" });
            spotkania.Add(new { liga = "Brazylia 2", sezon = "2019", adres = "https://www.betexplorer.com/soccer/brazil/serie-b/fixtures/" });
            spotkania.Add(new { liga = "Bułgaria 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/bulgaria/parva-liga/fixtures/" });
            spotkania.Add(new { liga = "Czechy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/czech-republic/1-liga/fixtures/" });
            spotkania.Add(new { liga = "Czechy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/czech-republic/division-2/fixtures/" });
            spotkania.Add(new { liga = "Dania 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/denmark/superliga/fixtures/" });
            spotkania.Add(new { liga = "Dania 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/denmark/1st-division/fixtures/" });
            spotkania.Add(new { liga = "Francja 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/france/ligue-1/fixtures/" });
            spotkania.Add(new { liga = "Francja 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/france/ligue-2/fixtures/" });
            spotkania.Add(new { liga = "Francja 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/france/national/fixtures/" });
            spotkania.Add(new { liga = "Hiszpania 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/spain/laliga/fixtures/" });
            spotkania.Add(new { liga = "Hiszpania 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/spain/laliga2/fixtures/" });
            spotkania.Add(new { liga = "Holandia 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/netherlands/eredivisie/fixtures/" });
            spotkania.Add(new { liga = "Holandia 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/netherlands/eerste-divisie/fixtures/" });
            spotkania.Add(new { liga = "Niemcy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/germany/bundesliga/fixtures/" });
            spotkania.Add(new { liga = "Niemcy 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/germany/2-bundesliga/fixtures/" });
            spotkania.Add(new { liga = "Niemcy 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/germany/3-liga/fixtures/" });
            spotkania.Add(new { liga = "Polska 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/poland/ekstraklasa/fixtures/" });
            spotkania.Add(new { liga = "Polska 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/poland/division-1/fixtures/" });
            spotkania.Add(new { liga = "Polska 3", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/poland/division-2/fixtures/" });
            spotkania.Add(new { liga = "Portugalia 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/portugal/primeira-liga/fixtures/" });
            spotkania.Add(new { liga = "Portugalia 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/portugal/ligapro/fixtures/" });
            spotkania.Add(new { liga = "Włochy 1", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/italy/serie-a/fixtures/" });
            spotkania.Add(new { liga = "Włochy 2", sezon = "2019/2020", adres = "https://www.betexplorer.com/soccer/italy/serie-b/fixtures/" });
        }
        #endregion

        private async void poniżej25ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = "SELECT liga, max(sezon) as sezon FROM p_wyniki GROUP BY liga ORDER BY liga";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        int akt = 0, max = 0, iter = 0, remis = 0;
                        string zLiga = czytnik.GetString(0);
                        string zSezon = czytnik.GetString(1);
                        podzapytanie = string.Format("SELECT gosp FROM p_wszystko WHERE liga='{0}' AND sezon='{1}' ORDER BY gosp", zLiga, zSezon);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows)
                        {
                            while (podczytnik.Read())
                            {
                                string zGosp = podczytnik.GetString(0);
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND sezon = '{2}' ORDER BY data DESC", zGosp, zGosp, zSezon);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc > 2)
                                        {
                                            if (remis == 0)
                                                akt++;
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                            remis = 1;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += zLiga + ";" + zGosp + ";" + akt + ";" + max;
                                }
                                akt = 0;
                                max = 0;
                                iter = 0;
                                remis = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2018/2019' OR sezon = '2018') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc > 2)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2017/2018' OR sezon = '2017') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc > 2)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2016/2017' OR sezon = '2016') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc > 2)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2015/2016' OR sezon = '2015') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc > 2)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2014/2015' OR sezon = '2014') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc > 2)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max + "";
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT TOP 1 data FROM p_spotkania WHERE (gosp='{0}' OR gosc='{1}') ORDER BY data", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    string zData = "";
                                    while (czytnik2.Read())
                                    {
                                        zData = czytnik2.GetDateTime(0).ToString("yyyy-MM-dd");
                                    }
                                    richTextBox1.Text += ";" + zData + "\n";
                                }
                                else
                                {
                                    richTextBox1.Text += "\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać ligi i max sezonu'{0}'.\n", ex.Message);
                MessageBox.Show(byk, "Błąd wybierania ligi i max sezonu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async void powyżej25ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = "SELECT liga, max(sezon) as sezon FROM p_wyniki GROUP BY liga ORDER BY liga";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        int akt = 0, max = 0, iter = 0, remis = 0;
                        string zLiga = czytnik.GetString(0);
                        string zSezon = czytnik.GetString(1);
                        podzapytanie = string.Format("SELECT gosp FROM p_wszystko WHERE liga='{0}' AND sezon='{1}' ORDER BY gosp", zLiga, zSezon);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows)
                        {
                            while (podczytnik.Read())
                            {
                                string zGosp = podczytnik.GetString(0);
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND sezon = '{2}' ORDER BY data DESC", zGosp, zGosp, zSezon);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc < 3)
                                        {
                                            if (remis == 0)
                                                akt++;
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                            remis = 1;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += zLiga + ";" + zGosp + ";" + akt + ";" + max;
                                }
                                akt = 0;
                                max = 0;
                                iter = 0;
                                remis = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2018/2019' OR sezon = '2018') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc < 3)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2017/2018' OR sezon = '2017') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc < 3)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2016/2017' OR sezon = '2016') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc < 3)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2015/2016' OR sezon = '2015') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc < 3)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2014/2015' OR sezon = '2014') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp + wGosc < 3)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max + "";
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT TOP 1 data FROM p_spotkania WHERE (gosp='{0}' OR gosc='{1}') ORDER BY data", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    string zData = "";
                                    while (czytnik2.Read())
                                    {
                                        zData = czytnik2.GetDateTime(0).ToString("yyyy-MM-dd");
                                    }
                                    richTextBox1.Text += ";" + zData + "\n";
                                }
                                else
                                {
                                    richTextBox1.Text += "\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać ligi i max sezonu'{0}'.\n", ex.Message);
                MessageBox.Show(byk, "Błąd wybierania ligi i max sezonu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async void ąToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = "SELECT liga, max(sezon) as sezon FROM p_wyniki GROUP BY liga ORDER BY liga";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        int akt = 0, max = 0, iter = 0, remis = 0;
                        string zLiga = czytnik.GetString(0);
                        string zSezon = czytnik.GetString(1);
                        podzapytanie = string.Format("SELECT gosp FROM p_wszystko WHERE liga='{0}' AND sezon='{1}' ORDER BY gosp", zLiga, zSezon);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows)
                        {
                            while (podczytnik.Read())
                            {
                                string zGosp = podczytnik.GetString(0);
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND sezon = '{2}' ORDER BY data DESC", zGosp, zGosp, zSezon);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp < 1 || wGosc < 1)
                                        {
                                            if (remis == 0)
                                                akt++;
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                            remis = 1;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += zLiga + ";" + zGosp + ";" + akt + ";" + max;
                                }
                                akt = 0;
                                max = 0;
                                iter = 0;
                                remis = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2018/2019' OR sezon = '2018') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp < 1 || wGosc < 1)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2017/2018' OR sezon = '2017') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp < 1 || wGosc < 1)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2016/2017' OR sezon = '2016') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp < 1 || wGosc < 1)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2015/2016' OR sezon = '2015') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp < 1 || wGosc < 1)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2014/2015' OR sezon = '2014') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp < 1 || wGosc < 1)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max + "";
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT TOP 1 data FROM p_spotkania WHERE (gosp='{0}' OR gosc='{1}') ORDER BY data", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    string zData = "";
                                    while (czytnik2.Read())
                                    {
                                        zData = czytnik2.GetDateTime(0).ToString("yyyy-MM-dd");
                                    }
                                    richTextBox1.Text += ";" + zData + "\n";
                                }
                                else
                                {
                                    richTextBox1.Text += "\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać ligi i max sezonu'{0}'.\n", ex.Message);
                MessageBox.Show(byk, "Błąd wybierania ligi i max sezonu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async void parzysteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = "SELECT liga, max(sezon) as sezon FROM p_wyniki GROUP BY liga ORDER BY liga";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        int akt = 0, max = 0, iter = 0, remis = 0;
                        string zLiga = czytnik.GetString(0);
                        string zSezon = czytnik.GetString(1);
                        podzapytanie = string.Format("SELECT gosp FROM p_wszystko WHERE liga='{0}' AND sezon='{1}' ORDER BY gosp", zLiga, zSezon);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows)
                        {
                            while (podczytnik.Read())
                            {
                                string zGosp = podczytnik.GetString(0);
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND sezon = '{2}' ORDER BY data DESC", zGosp, zGosp, zSezon);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 != 0)
                                        {
                                            if (remis == 0)
                                                akt++;
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                            remis = 1;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += zLiga + ";" + zGosp + ";" + akt + ";" + max;
                                }
                                akt = 0;
                                max = 0;
                                iter = 0;
                                remis = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2018/2019' OR sezon = '2018') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 != 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2017/2018' OR sezon = '2017') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 != 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2016/2017' OR sezon = '2016') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 != 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2015/2016' OR sezon = '2015') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 != 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2014/2015' OR sezon = '2014') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 != 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max + "";
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT TOP 1 data FROM p_spotkania WHERE (gosp='{0}' OR gosc='{1}') ORDER BY data", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    string zData = "";
                                    while (czytnik2.Read())
                                    {
                                        zData = czytnik2.GetDateTime(0).ToString("yyyy-MM-dd");
                                    }
                                    richTextBox1.Text += ";" + zData + "\n";
                                }
                                else
                                {
                                    richTextBox1.Text += "\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać ligi i max sezonu'{0}'.\n", ex.Message);
                MessageBox.Show(byk, "Błąd wybierania ligi i max sezonu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async void nieparzysteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = "SELECT liga, max(sezon) as sezon FROM p_wyniki GROUP BY liga ORDER BY liga";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        int akt = 0, max = 0, iter = 0, remis = 0;
                        string zLiga = czytnik.GetString(0);
                        string zSezon = czytnik.GetString(1);
                        podzapytanie = string.Format("SELECT gosp FROM p_wszystko WHERE liga='{0}' AND sezon='{1}' ORDER BY gosp", zLiga, zSezon);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows)
                        {
                            while (podczytnik.Read())
                            {
                                string zGosp = podczytnik.GetString(0);
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND sezon = '{2}' ORDER BY data DESC", zGosp, zGosp, zSezon);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 == 0)
                                        {
                                            if (remis == 0)
                                                akt++;
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                            remis = 1;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += zLiga + ";" + zGosp + ";" + akt + ";" + max;
                                }
                                akt = 0;
                                max = 0;
                                iter = 0;
                                remis = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2018/2019' OR sezon = '2018') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 == 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2017/2018' OR sezon = '2017') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 == 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2016/2017' OR sezon = '2016') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 == 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2015/2016' OR sezon = '2015') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 == 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2014/2015' OR sezon = '2014') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if ((wGosp + wGosc) % 2 == 0)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max + "";
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT TOP 1 data FROM p_spotkania WHERE (gosp='{0}' OR gosc='{1}') ORDER BY data", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    string zData = "";
                                    while (czytnik2.Read())
                                    {
                                        zData = czytnik2.GetDateTime(0).ToString("yyyy-MM-dd");
                                    }
                                    richTextBox1.Text += ";" + zData + "\n";
                                }
                                else
                                {
                                    richTextBox1.Text += "\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać ligi i max sezonu'{0}'.\n", ex.Message);
                MessageBox.Show(byk, "Błąd wybierania ligi i max sezonu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async void RemisyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = "SELECT liga, max(sezon) as sezon FROM p_wyniki GROUP BY liga ORDER BY liga";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        int akt = 0, max = 0, iter = 0, remis = 0;
                        string zLiga = czytnik.GetString(0);
                        string zSezon = czytnik.GetString(1);
                        podzapytanie = string.Format("SELECT gosp FROM p_wszystko WHERE liga='{0}' AND sezon='{1}' ORDER BY gosp", zLiga, zSezon);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows)
                        {
                            while (podczytnik.Read())
                            {                                
                                string zGosp = podczytnik.GetString(0);
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND sezon = '{2}' ORDER BY data DESC", zGosp, zGosp, zSezon);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp != wGosc)
                                        {
                                            if (remis == 0)
                                                akt++;
                                            iter++;
                                        } else
                                        {                                            
                                            iter = 0;
                                            remis = 1;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += zLiga + ";" + zGosp + ";" + akt + ";" + max;
                                }
                                akt = 0;
                                max = 0;
                                iter = 0;
                                remis = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2018/2019' OR sezon = '2018') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp != wGosc)
                                        {                                            
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                } else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2017/2018' OR sezon = '2017') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp != wGosc)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2016/2017' OR sezon = '2016') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp != wGosc)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2015/2016' OR sezon = '2015') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp != wGosc)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max;
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND (sezon = '2014/2015' OR sezon = '2014') ORDER BY data DESC", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    while (czytnik2.Read())
                                    {
                                        short wGosp = czytnik2.GetInt16(2);
                                        short wGosc = czytnik2.GetInt16(3);
                                        if (wGosp != wGosc)
                                        {
                                            iter++;
                                        }
                                        else
                                        {
                                            iter = 0;
                                        }
                                        if (max < iter)
                                            max = iter;
                                    }
                                    richTextBox1.Text += ";" + max + "";
                                }
                                else
                                {
                                    richTextBox1.Text += ";0";
                                }
                                max = 0;
                                iter = 0;
                                zapytanie2 = string.Format("SELECT TOP 1 data FROM p_spotkania WHERE (gosp='{0}' OR gosc='{1}') ORDER BY data", zGosp, zGosp);
                                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                                czytnik2 = await komenda2.ExecuteReaderAsync();
                                if (czytnik2.HasRows)
                                {
                                    string zData = "";
                                    while (czytnik2.Read())
                                    {
                                        zData = czytnik2.GetDateTime(0).ToString("yyyy-MM-dd");
                                    }
                                    richTextBox1.Text += ";" + zData + "\n";
                                }
                                else
                                {
                                    richTextBox1.Text += "\n";
                                }
                            }                            
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać ligi i max sezonu'{0}'.\n", ex.Message);
                MessageBox.Show(byk, "Błąd wybierania ligi i max sezonu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private void WyczyscListy()
        {
            dane.Clear();
            wyniki.Clear();
            wynikSz.Clear();
            przygotowaneDane.Clear();
            DaneLigaSezon.Clear();            
            daneOstateczneSpotkania.Clear();
            daneSpotkania.Clear();
            listaSpotkania.Clear();
            DaneLigaGospSezon.Clear();
            gosp = "";
        }

        private async void BtnNozna_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            foreach (var item in adresy)
            {
                WyczyscListy();
                sezon = "";
                status.Text = "Pobieram wyniki ligi: " + item.liga;
                zrodlo = await PobierzDane(item.adres);
                await WybierzSezon(zrodlo);
                await SkrocZrodlo(zrodlo, item.liga, sezon);
                if (ileRekordow > 1)
                {
                    await PobierzWyniki(item.liga);                
                    PrzygotujDane();
                    await DodajDaneDoBazy();
                }
            }
            status.Text = "Obliczenia...";
            await CzyszczenieTabel();
            await PobierzDaneLigaSezon();
            await PobierzDaneLigaGospSezon("gosp");
            await PobierzDaneLigaGospSezon("gosc");
            await TworzenieTabel();
            await PrzypiszMiejscaTabeli(0, "p_dom_tmp", "p_dom");
            await PrzypiszMiejscaTabeli(0, "p_wyjazd_tmp", "p_wyjazd");
            await PrzypiszMiejscaTabeli(0, "p_wszystko_tmp", "p_wszystko");

            await UsunSpotkania();
            await PrzygotujDaneSpotkania();
            await DodajSpotkaniaDoBazy();
            status.Text = "Gotowy";
        }

        private async Task<string> PobierzDane(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2");
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return content;
                    }
                }
            }
        }

        private async Task WybierzSezon(string zrodlo)
        {
            sezon = "";
            StringReader sr = new StringReader(zrodlo);
            while ((line = await sr.ReadLineAsync()) != null)
            {
                // liczymy ile jest najnowszych spotkań w pobranym źródle
                if ((spr = line.IndexOf("selected=")) > 0)
                {
                    start = line.ToString().IndexOf("selected=");
                    end = line.ToString().IndexOf(">", start);
                    start = line.ToString().IndexOf("</", end);
                    if (sezon.Length == 0) // jeżeli sezon jest nieustawiony
                        sezon = line.ToString().Substring(end + 1, start - end - 1);
                    break;
                }
            }
        }

        private async Task SkrocZrodlo(string zrodlo, string zLiga, string zSezon)
        {
            // najpierw policzymy ile jest już spotkań w bazie z najnowszego sezonu
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = string.Format("SELECT count(*) FROM p_wyniki WHERE liga='{0}' AND sezon='{1}'", zLiga, zSezon);
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();
                ileSpotkanBaza = 0;
                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        ileSpotkanBaza = czytnik.GetInt32(0); // przykładowo 20
                    }
                }  
                else
                {
                    ileSpotkanBaza = 0;
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę policzyć aktualnych spotkań z ligi '{0}'.\n{1}", zLiga, ex.Message);
                MessageBox.Show(byk, "Błąd liczenia spotkań", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            int licz = 0;
            StringReader sr = new StringReader(zrodlo);
            while ((line = await sr.ReadLineAsync()) != null)
            {
                // liczymy ile jest najnowszych spotkań w pobranym źródle
                if ((spr = line.IndexOf("selected=")) > 0 || (spr = line.IndexOf("h-text-left")) > 0)
                {
                    if ((spr = line.IndexOf("colspan")) < 0 && (spr = line.IndexOf("selected=")) < 0 && (spr = line.IndexOf("POSTP.")) < 0)
                    {
                        licz++; // przykładowo 22
                    }
                }
            }
            int ileNowychSpotkań = licz - ileSpotkanBaza;
            if (ileNowychSpotkań > 1)
                ileRekordow = ileNowychSpotkań;
            else
                ileRekordow = 0;

            if (ileRekordow > 1)
            {   
                StringReader sr2 = new StringReader(zrodlo);
                while ((line = await sr2.ReadLineAsync()) != null)
                {
                    int ust = 0;
                    // liczymy ile jest najnowszych spotkań w pobranym źródle
                    if ((spr = line.IndexOf("selected=")) > 0 || (spr = line.IndexOf("h-text-left")) > 0)
                    {
                        ust = 1;
                        if ((spr = line.IndexOf("colspan")) < 0 && (spr = line.IndexOf("selected=")) < 0)
                        {
                            dane.Add(line);
                            ileNowychSpotkań--;
                            ust = 0;
                        }

                        if (ileNowychSpotkań == 0)
                            break;

                        if (ust == 1)
                            dane.Add(line);
                    }
                }
            }
        }

        private async Task SkrocZrodlo2(string zrodlo)
        {            
            StringReader sr = new StringReader(zrodlo);
            while ((line = await sr.ReadLineAsync()) != null)
            {
                // liczymy ile jest najnowszych spotkań w pobranym źródle
                if ((spr = line.IndexOf("selected=")) > 0 || (spr = line.IndexOf("h-text-left")) > 0)
                {
                    dane.Add(line);                    
                }
            }            
        }

        private async Task SkrocZrodloWynik(string zrodlo)
        {
            StringReader sr = new StringReader(zrodlo);
            while ((line = await sr.ReadLineAsync()) != null)
            {
                if ((spr = line.IndexOf("js-partial")) > 0 )
                {
                    wynikSz.Add(line);
                }
            }
        }

        private void PobierzSezon()
        {
            sezon = "";
            for (int i = 0; i < dane.Count - 1; i++)
            {
                if ((spr = dane[i].ToString().IndexOf("selected=")) > 0)
                {
                    start = dane[i].ToString().IndexOf("selected=");
                    end = dane[i].ToString().IndexOf(">", start);
                    start = dane[i].ToString().IndexOf("</", end);
                    if (sezon.Length == 0) // jeżeli sezon jest nieustawiony
                        sezon = dane[i].ToString().Substring(end + 1, start - end - 1);
                    break;
                }                
            }
        }

        private async Task PobierzWyniki(string liga)
        {
            wyniki.Clear();
            int awans = 0;
            for (int i = 0; i < dane.Count; i++)
            {
                if ((spr = dane[i].ToString().IndexOf("h-text-left\" c")) > 0)
                {
                    start = dane[i].ToString().IndexOf("h-text-left\" c");
                    end = dane[i].ToString().IndexOf(">", start);
                    start = dane[i].ToString().IndexOf(".", end);
                    runda = Convert.ToInt32(dane[i].ToString().Substring(end + 1, start - end - 1));
                    status.Text = "Pobieram wyniki ligi: " + liga + " z sezonu: " + sezon + " - runda: " + runda;

                    // zeruję gosp, żeby gospodarz z rundy poprzedniej nie był dodawany po zmianie rundy
                    gosp = "";
                    i++;
                }

                int czySpotkanie = 0;
                // pobranie adresu do dokładnego wyniku
                if ((spr = dane[i].ToString().IndexOf("a href=")) > 0 && runda > 0)
                {
                    end = dane[i].ToString().IndexOf("a href=");
                    start = dane[i].ToString().IndexOf(" class=", end);
                    link = dane[i].Substring(end + 8, start - end - 9);
                    string source = await PobierzDane("https://www.betexplorer.com" + link);
                    await SkrocZrodloWynik(source);

                    if ((spr = wynikSz[0].ToString().IndexOf("js-partial")) > 0)
                    {
                        start = wynikSz[0].ToString().IndexOf("(");
                        awans = wynikSz[0].ToString().IndexOf("&");
                        if (start > 0)
                        {
                            end = wynikSz[0].ToString().IndexOf(":", start);
                            w1gosp = Convert.ToInt16(wynikSz[0].ToString().Substring(start + 1, end - start - 1));
                            start = wynikSz[0].ToString().IndexOf(",", end);
                            w1gosc = Convert.ToInt16(wynikSz[0].ToString().Substring(end + 1, start - end - 1));
                            end = wynikSz[0].ToString().IndexOf(":", start);
                            w2gosp = Convert.ToInt16(wynikSz[0].ToString().Substring(start + 2, end - start - 2));
                            start = wynikSz[0].ToString().IndexOf(")", end);
                            w2gosc = Convert.ToInt16(wynikSz[0].ToString().Substring(end + 1, start - end - 1));
                            czySpotkanie = 0;
                            wynikSz.Clear();
                        }
                        else
                        {
                            czySpotkanie = 1;
                        }
                    }
                }

                // pobranie gospodarza
                if ((spr = dane[i].ToString().IndexOf("<td class=")) > 0)
                {
                    start = dane[i].ToString().IndexOf("<td class=");
                    end = dane[i].ToString().IndexOf("<span>", start);
                    start = dane[i].ToString().IndexOf("</span>", end);
                    if (dane[i].ToString().Substring(end + 6, 1).Equals("<"))
                    {
                        gosp = dane[i].ToString().Substring(end + 14, start - end - 23);
                    }
                    else
                    {
                        gosp = dane[i].ToString().Substring(end + 6, start - end - 6);
                    }

                    // pobranie gości
                    end = dane[i].ToString().IndexOf("<span>", start);
                    start = dane[i].ToString().IndexOf("</span>", end);
                    if (dane[i].ToString().Substring(end + 6, 2).Equals("<s"))
                    {
                        gosc = dane[i].ToString().Substring(end + 14, start - end - 23);
                    }
                    else
                    {
                        gosc = dane[i].ToString().Substring(end + 6, start - end - 6);
                    }

                    // wynik gospodarza
                    end = dane[i].ToString().IndexOf("<a href", start);
                    start = dane[i].ToString().IndexOf(">", end);
                    end = dane[i].ToString().IndexOf(":", start);
                    if (end < 0)
                    {
                        czyWynik = 1;
                    }
                    else
                    {
                        czyWynik = 0;
                        wgosp = Convert.ToInt32(dane[i].ToString().Substring(start + 1, end - start - 1));
                    }

                    // wynik gości
                    if (czyWynik == 0)
                    {
                        start = dane[i].ToString().IndexOf("<", end);
                        wgosc = Convert.ToInt32(dane[i].ToString().Substring(end + 1, start - end - 1));
                    }

                    if (awans > 0)
                    {
                        w1gosp = wgosp;
                        w1gosc = wgosc;
                        w2gosp = 0;
                        w2gosc = 0;
                        wynikSz.Clear();
                        czySpotkanie = 0;
                    }

                    // kurs gospodarzy
                    if (czyWynik == 0)
                    {
                        end = dane[i].ToString().IndexOf("data-odd", start);
                        if (end > 0)
                        {
                            start = dane[i].ToString().IndexOf(">", end);
                            kursGosp = dane[i].ToString().Substring(end + 10, start - end - 11);
                        }
                        else
                        {
                            kursGosp = "0.00";
                        }
                    }

                    // kurs dla remisu
                    if (czyWynik == 0)
                    {
                        end = dane[i].ToString().IndexOf("data-odd", start);
                        if (end > 0)
                        {
                            start = dane[i].ToString().IndexOf(">", end);
                            kursRemis = dane[i].ToString().Substring(end + 10, start - end - 11);
                        }
                        else
                        {
                            kursRemis = "0.00";
                        }
                    }

                    // kurs dla gości
                    if (czyWynik == 0)
                    {
                        end = dane[i].ToString().IndexOf("data-odd", start);
                        if (end > 0)
                        {
                            start = dane[i].ToString().IndexOf(">", end);
                            kursGosc = dane[i].ToString().Substring(end + 10, start - end - 11);
                        }
                        else
                        {
                            kursGosc = "0.00";
                        }
                    }

                    // data spotkania
                    if (czyWynik == 0)
                    {
                        string rok = DateTime.Today.ToString("yyyy");
                        end = dane[i].ToString().IndexOf("no-wrap");
                        if (dane[i].ToString().Substring(end + 9, 5).Equals("Today"))
                            data = DateTime.Today.ToString("yyyy-MM-dd");
                        else if (dane[i].ToString().Substring(end + 9, 9).Equals("Yesterday"))
                            data = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                        else if (dane[i].ToString().Substring(end + 15, 1).Equals("<"))
                        {
                            data = rok + "-" + dane[i].ToString().Substring(end + 12, 2) + "-" + dane[i].ToString().Substring(end + 9, 2);
                        }
                        else
                        {
                            data = dane[i].ToString().Substring(end + 15, 4) + "-" + dane[i].ToString().Substring(end + 12, 2) + "-" + dane[i].ToString().Substring(end + 9, 2);
                        }
                    }
                }
                if (runda > 0 && gosp.Length > 0 && czyWynik == 0 && czySpotkanie == 0)
                    wyniki.Add(data + ";" + liga + ";" + sezon + ";" + runda + ";" + gosp + ";" + gosc + ";" + wgosp + ";" + wgosc + ";" + w1gosp + ";" + w1gosc + ";" + w2gosp + ";" + w2gosc + ";" + kursGosp + ";" + kursRemis + ";" + kursGosc);
            }
        }

        private async Task IloscSpotkan(string zLiga, string zSezon)
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                // pobranie spotkań z bazy
                zapytanie = string.Format("SELECT count(*) FROM p_wyniki WHERE liga='{0}' AND sezon='{1}'", zLiga, zSezon);
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();

                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        ileSpotkanBaza = czytnik.GetInt32(0);
                    }
                }
                ileRekordow = wyniki.Count - ileSpotkanBaza;
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę policzyć aktualnie dodanych spotkań z ligi '{0}'.\n{1}", zLiga, ex.Message);
                MessageBox.Show(byk, "Błąd liczenia spotkań", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrzygotujDane()
        {
            if (wyniki.Count > 0)
            {
                for (int i = 0; i < wyniki.Count; i++)
                {
                    przygotowaneDane.Add(wyniki[i]);
                }
            }
        }

        public async Task DodajDaneDoBazy()
        {
            status.Text = "Dodawanie spotkań do bazy.";
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                if (przygotowaneDane.Count > 0)
                {
                    foreach (var item in przygotowaneDane)
                    {
                        var dane = item.Split(';');
                        dane[4] = dane[4].Replace("'", "''");
                        dane[5] = dane[5].Replace("'", "''");
                        zapytanie = string.Format("INSERT INTO p_wyniki (data, liga, sezon, runda, gosp, gosc, wgosp, wgosc, w1gosp, w1gosc, w2gosp, w2gosc, kursGosp, kursRemis, kursGosc) VALUES('{0}', '{1}', '{2}', {3}, '{4}', '{5}', {6}, {7}, {8}, {9}, {10}, {11}, '{12}', '{13}', '{14}')", dane[0], dane[1], dane[2], dane[3], dane[4], dane[5], dane[6], dane[7], dane[8], dane[9], dane[10], dane[11], dane[12], dane[13], dane[14]);
                        komenda = new SqlCommand(zapytanie, polaczenie);
                        await komenda.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    status.Text = "Dla ligi: " + txtLiga + " i sezonu: " + sezon + " - brak nowych spotkań";                    
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę dodać spotkań do bazy.\n{0}", ex.Message);
                MessageBox.Show(byk, "Błąd dodawania spotkań", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
            if (przygotowaneDane.Count > 0)
            {
                status.Text = string.Format("Dla ligi: {0} i sezonu: {1} - dodano {2} spotkania.", txtLiga, sezon, przygotowaneDane.Count);
            }            
        }

        private async Task PobierzDaneLigaSezon()
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                zapytanie = "SELECT liga,sezon FROM p_wyniki group by liga, sezon order by liga, sezon;";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();
                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        DaneLigaSezon.Add(czytnik.GetString(0) + ";" + czytnik.GetString(1));
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę pobrać lig i sezonów.\n{0}", ex.Message);
                MessageBox.Show(byk, "Błąd pobierania", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async Task WyczyscTabele(string nazwa)
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                zapytanie = string.Format("DELETE FROM {0};", nazwa);
                komenda = new SqlCommand(zapytanie, polaczenie);
                await komenda.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę usunąć danych z tabeli: '{0}'\n'{1}'", nazwa, ex.Message);
                MessageBox.Show(byk, "Błąd usuwania danych z tabel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async Task CzyszczenieTabel()
        {
            await WyczyscTabele("p_zespoly");
            await WyczyscTabele("p_dom_tmp");
            await WyczyscTabele("p_wyjazd_tmp");
            await WyczyscTabele("p_wszystko_tmp");
            await WyczyscTabele("p_dom");
            await WyczyscTabele("p_wyjazd");
            await WyczyscTabele("p_wszystko");
        }

        private async Task PobierzDaneLigaGospSezon(string gosp)
        {
            foreach (var item in DaneLigaSezon)
            {
                try
                {
                    if (polaczenie.State == ConnectionState.Closed)
                        polaczenie.Open();

                    var dane = item.Split(';');
                    zapytanie = string.Format("SELECT distinct({0}) FROM p_wyniki WHERE liga='{1}' AND sezon='{2}';", gosp, dane[0], dane[1]);
                    komenda = new SqlCommand(zapytanie, polaczenie);
                    czytnik = await komenda.ExecuteReaderAsync();
                    if (czytnik.HasRows)
                    {
                        while (czytnik.Read())
                        {
                            string gospodarz = czytnik.GetString(0);
                            gospodarz = gospodarz.Replace("'", "''");
                            DaneLigaGospSezon.Add(string.Format("{0};{1};{2}", dane[0], gospodarz, dane[1]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string byk = string.Format("Nie mogę pobrać gospodarzy.\n{0}", ex.Message);
                    MessageBox.Show(byk, "Błąd pobierania gospodarzy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    polaczenie.Close();
                }
            }
        }

        private async Task TworzenieTabel()
        {
            status.Text = "Obliczenia...";
            foreach (var item in DaneLigaGospSezon)
            {
                try
                {
                    if (polaczenie.State == ConnectionState.Closed)
                        polaczenie.Open();

                    var dane = item.Split(';');
                    zapytanie = string.Format("SELECT gosp, gosc, wgosp, wgosc FROM p_wyniki WHERE (gosp='{0}' OR gosc='{1}') AND liga='{2}' AND sezon='{3}'", dane[1], dane[1], dane[0], dane[2]);
                    komenda = new SqlCommand(zapytanie, polaczenie);
                    czytnik = await komenda.ExecuteReaderAsync();
                    int ile = 0, dile = 0, aile = 0;
                    int wygrane = 0, remisy = 0, przegrane = 0;
                    int dwygrane = 0, dremisy = 0, dprzegrane = 0;
                    int awygrane = 0, aremisy = 0, aprzegrane = 0;
                    int strzelone = 0, stracone = 0, roznica = 0, pkt = 0;
                    int dstrzelone = 0, dstracone = 0, droznica = 0, dpkt = 0;
                    int astrzelone = 0, astracone = 0, aroznica = 0, apkt = 0;
                    if (czytnik.HasRows)
                    {
                        while (czytnik.Read())
                        {
                            string tgosp = czytnik.GetString(0);
                            string tgosc = czytnik.GetString(1);
                            int wgosp = czytnik.GetInt16(2);
                            int wgosc = czytnik.GetInt16(3);

                            if (tgosp.Equals(dane[1]))
                            {
                                if (wgosp > wgosc)
                                {
                                    ile++;
                                    dile++;
                                    wygrane++;
                                    strzelone = strzelone + wgosp;
                                    stracone = stracone + wgosc;
                                    dwygrane++;
                                    dstrzelone = dstrzelone + wgosp;
                                    dstracone = dstracone + wgosc;
                                    roznica = strzelone - stracone;
                                    droznica = dstrzelone - dstracone;
                                    pkt = pkt + 3;
                                    dpkt = dpkt + 3;
                                }
                                else if (wgosp == wgosc)
                                {
                                    ile++;
                                    dile++;
                                    remisy++;
                                    strzelone = strzelone + wgosp;
                                    stracone = stracone + wgosc;
                                    dremisy++;
                                    dstrzelone = dstrzelone + wgosp;
                                    dstracone = dstracone + wgosc;
                                    roznica = strzelone - stracone;
                                    droznica = dstrzelone - dstracone;
                                    pkt = pkt + 1;
                                    dpkt = dpkt + 1;
                                }
                                else if (wgosp < wgosc)
                                {
                                    ile++;
                                    dile++;
                                    przegrane++;
                                    strzelone = strzelone + wgosp;
                                    stracone = stracone + wgosc;
                                    dprzegrane++;
                                    dstrzelone = dstrzelone + wgosp;
                                    dstracone = dstracone + wgosc;
                                    roznica = strzelone - stracone;
                                    droznica = dstrzelone - dstracone;
                                }
                            }
                            else if (tgosc.Equals(dane[1]))
                            {
                                if (wgosp > wgosc)
                                {
                                    ile++;
                                    aile++;
                                    przegrane++;
                                    strzelone = strzelone + wgosc;
                                    stracone = stracone + wgosp;
                                    aprzegrane++;
                                    astrzelone = astrzelone + wgosc;
                                    astracone = astracone + wgosp;
                                    roznica = strzelone - stracone;
                                    aroznica = astrzelone - astracone;
                                }
                                else if (wgosp == wgosc)
                                {
                                    ile++;
                                    aile++;
                                    remisy++;
                                    strzelone = strzelone + wgosc;
                                    stracone = stracone + wgosp;
                                    aremisy++;
                                    astrzelone = astrzelone + wgosc;
                                    astracone = astracone + wgosp;
                                    roznica = strzelone - stracone;
                                    aroznica = astrzelone - astracone;
                                    pkt = pkt + 1;
                                    apkt = apkt + 1;
                                }
                                else if (wgosp < wgosc)
                                {
                                    ile++;
                                    aile++;
                                    wygrane++;
                                    strzelone = strzelone + wgosc;
                                    stracone = stracone + wgosp;
                                    awygrane++;
                                    astrzelone = astrzelone + wgosc;
                                    astracone = astracone + wgosp;
                                    roznica = strzelone - stracone;
                                    aroznica = astrzelone - astracone;
                                    pkt = pkt + 3;
                                    apkt = apkt + 3;
                                }
                            }
                        }
                    }
                    dane[1] = dane[1].Replace("'", "''");
                    await TabelaTmp("p_dom_tmp", dane[0], dane[2], dane[1], dile, dwygrane, dremisy, dprzegrane, dstrzelone, dstracone, droznica, dpkt);
                    await TabelaTmp("p_wyjazd_tmp", dane[0], dane[2], dane[1], aile, awygrane, aremisy, aprzegrane, astrzelone, astracone, aroznica, apkt);
                    await TabelaTmp("p_wszystko_tmp", dane[0], dane[2], dane[1], ile, wygrane, remisy, przegrane, strzelone, stracone, roznica, pkt);
                }
                catch (Exception ex)
                {
                    string byk = string.Format("Nie mogę wybrać danych.\n{0}", ex.Message);
                    MessageBox.Show(byk, "Błąd pobierania danych", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    polaczenie.Close();
                }
            }
        }

        private async Task TabelaTmp(string tabela, string liga, string sezon, string gosp, int ile, int wygrane, int remisy, int przegrane, int strzelone, int stracone, int roznica, int pkt)
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                zapytanie2 = string.Format("INSERT INTO {0} (liga,sezon,gosp,ile,wygrane,remisy,przegrane,strzelone,stracone,roznica,pkt) VALUES('{1}','{2}','{3}',{4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});", tabela, liga, sezon, gosp, ile, wygrane, remisy, przegrane, strzelone, stracone, roznica, pkt);
                komenda2 = new SqlCommand(zapytanie2, polaczenie);
                await komenda2.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wybrać danych.\n{0}", ex.Message);
                MessageBox.Show(byk, "Błąd pobierania danych", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async Task PrzypiszMiejscaTabeli(int miejsce, string tabelaZrodlo, string tabela)
        {
            foreach (var item in DaneLigaSezon)
            {
                var dane = item.Split(';');
                try
                {
                    if (polaczenie.State == ConnectionState.Closed)
                        polaczenie.Open();

                    zapytanie = string.Format("SELECT * FROM {0} WHERE liga='{1}' AND sezon='{2}' ORDER BY pkt DESC, roznica DESC;", tabelaZrodlo, dane[0], dane[1]);
                    komenda = new SqlCommand(zapytanie, polaczenie);
                    czytnik = await komenda.ExecuteReaderAsync();
                    if (czytnik.HasRows)
                    {
                        while (czytnik.Read())
                        {
                            miejsce++;
                            string gosp = czytnik.GetString(2);
                            int ile = czytnik.GetInt16(3);
                            int wygrane = czytnik.GetInt16(4);
                            int remisy = czytnik.GetInt16(5);
                            int przegrane = czytnik.GetInt16(6);
                            int strzelone = czytnik.GetInt16(7);
                            int stracone = czytnik.GetInt16(8);
                            int roznica = czytnik.GetInt16(9);
                            int pkt = czytnik.GetInt16(10);
                            await GotowaTabela(tabela, miejsce, dane[0], dane[1], gosp, ile, wygrane, remisy, przegrane, strzelone, stracone, roznica, pkt);
                        }
                        miejsce = 0;
                    }
                }
                catch (Exception ex)
                {
                    string byk = string.Format("Nie mogę przypisać miejsc w tabeli.\n{0}", ex.Message);
                    MessageBox.Show(byk, "Błąd przypisywania miejsc", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    polaczenie.Close();
                }
            }
        }

        private async Task GotowaTabela(string tabela, int miejsce, string liga, string sezon, string gosp, int ile, int wygrane, int remisy, int przegrane, int strzelone, int stracone, int roznica, int pkt)
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                zapytanie = string.Format("INSERT INTO {0} VALUES({1}, '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12});", tabela, miejsce, liga, sezon, gosp, ile, wygrane, remisy, przegrane, strzelone, stracone, roznica, pkt);
                komenda = new SqlCommand(zapytanie, polaczenie);
                await komenda.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę utworzyć tabeli: {0}.\n{1}", tabela, ex.Message);
                MessageBox.Show(byk, "Błąd tworzenia tabeli", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UsunSpotkania()
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                zapytanie = "DELETE FROM p_spotkania";
                komenda = new SqlCommand(zapytanie, polaczenie);
                await komenda.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę usunąć spotkań z bazy.\n{0}", ex.Message);
                MessageBox.Show(byk, "Błąd usuwania spotkań", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
        }

        private async Task PrzygotujDaneSpotkania()
        {
            foreach (var item in spotkania)
            {
                string source = await PobierzDane(item.adres);
                await SkrocSpotkania(source);
                PobierzSezonSpotkania(item.sezon);
                // w zmiennej przygotowaneDane przechowuję wszystkie pobrane wyniki spotkań z danej ligi
                status.Text = "Pobieram spotkania dla ligi: " + item.liga;
                PobierzSpotkania(item.liga);
            }
        }

        private async Task SkrocSpotkania(string zrodlo)
        {
            listaSpotkania.Clear();
            int poczatek = 0, koniec = 0;
            StringReader sr = new StringReader(zrodlo);
            while ((line = await sr.ReadLineAsync()) != null)
            {
                if ((spr = line.IndexOf("h-text-left")) > 0)
                {
                    poczatek = 1;
                }
                if (poczatek == 1 && koniec == 0)
                {
                    listaSpotkania.Add(line);
                }
                if ((spr = line.IndexOf("</section>")) > 0)
                {
                    koniec = 1;
                }
            }
        }

        private string PobierzSezonSpotkania(string sezon)
        {
            this.sezon = sezon;
            return sezon;
        }

        public List<string> PobierzSpotkania(string liga)
        {
            for (int i = 2; i < listaSpotkania.Count - 1; i++)
            {
                // data spotkania
                if ((spr = listaSpotkania[i].ToString().IndexOf("table-main__datetime")) > 0)
                {
                    string rok = DateTime.Today.ToString("yyyy");
                    end = listaSpotkania[i].ToString().IndexOf("table-main__datetime");
                    if (listaSpotkania[i].ToString().Substring(end + 22, 5).Equals("Today"))
                        data = DateTime.Today.ToString("yyyy-MM-dd");
                    else if (listaSpotkania[i].ToString().Substring(end + 22, 8).Equals("Tomorrow"))
                        data = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                    else if (listaSpotkania[i].ToString().Substring(end + 22, 1).Equals("&"))
                    {
                        data = data;
                    }
                    else if (listaSpotkania[i].ToString().Substring(end + 28, 1).Equals(" "))
                    {
                        data = rok + "-" + listaSpotkania[i].ToString().Substring(end + 25, 2) + "-" + listaSpotkania[i].ToString().Substring(end + 22, 2);
                    }
                    else
                    {
                        data = listaSpotkania[i].ToString().Substring(end + 28, 4) + "-" + listaSpotkania[i].ToString().Substring(end + 25, 2) + "-" + listaSpotkania[i].ToString().Substring(end + 22, 2);
                    }
                    i += 1;
                }

                // pobranie gospodarza
                if ((spr = listaSpotkania[i].ToString().IndexOf("td class=\"h")) > 0)
                {
                    start = listaSpotkania[i].ToString().IndexOf("<td class=\"h");
                    end = listaSpotkania[i].ToString().IndexOf("<span>", start);
                    start = listaSpotkania[i].ToString().IndexOf("</span>", end);

                    gosp = listaSpotkania[i].ToString().Substring(end + 6, start - end - 6);
                    end = listaSpotkania[i].ToString().IndexOf("<span>", start);
                    start = listaSpotkania[i].ToString().IndexOf("</span>", end);
                    gosc = listaSpotkania[i].ToString().Substring(end + 6, start - end - 6);
                    while ((spr = listaSpotkania[i].ToString().IndexOf("table-main__odds")) < 0)
                    {
                        i++;
                    }
                }

                // kurs gospodarzy
                if ((spr = listaSpotkania[i].ToString().IndexOf("table-main__odds")) > 0)
                {
                    if ((spr = listaSpotkania[i].ToString().IndexOf("data-odd")) > 0)
                    {
                        end = listaSpotkania[i].ToString().IndexOf("data-odd", start);
                        start = listaSpotkania[i].ToString().IndexOf(">", end);
                        kursGosp = listaSpotkania[i].ToString().Substring(end + 10, start - end - 11);
                    }
                    else
                    {
                        kursGosp = "0.00";
                    }
                    i += 1;
                }
                if ((spr = listaSpotkania[i].ToString().IndexOf("table-main__odds")) > 0)
                {
                    if ((spr = listaSpotkania[i].ToString().IndexOf("data-odd")) > 0)
                    {
                        end = listaSpotkania[i].ToString().IndexOf("data-odd");
                        start = listaSpotkania[i].ToString().IndexOf(">", end);
                        kursRemis = listaSpotkania[i].ToString().Substring(end + 10, start - end - 11);
                    }
                    else
                    {
                        kursRemis = "0.00";
                    }
                    i += 1;
                }
                if ((spr = listaSpotkania[i].ToString().IndexOf("table-main__odds")) > 0)
                {
                    if ((spr = listaSpotkania[i].ToString().IndexOf("data-odd")) > 0)
                    {
                        end = listaSpotkania[i].ToString().IndexOf("data-odd");
                        start = listaSpotkania[i].ToString().IndexOf(">", end);
                        kursGosc = listaSpotkania[i].ToString().Substring(end + 10, start - end - 11);
                    }
                    else
                    {
                        kursGosc = "0.00";
                    }
                }

                if (gosp.Length > 0)
                {
                    gosp = gosp.Replace("'", "''");
                    gosc = gosc.Replace("'", "''");
                    daneSpotkania.Add(data + ";" + liga + ";" + sezon + ";" + gosp + ";" + gosc + ";" + kursGosp + ";" + kursRemis + ";" + kursGosc);
                    while ((spr = listaSpotkania[i].ToString().IndexOf("table-main__datetime")) < 0)
                    {
                        i++;
                        if (i == listaSpotkania.Count - 1)
                            break;
                    }
                    i--;
                }
            }
            return daneSpotkania;
        }

        private async Task DodajSpotkaniaDoBazy()
        {
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();

                if (daneSpotkania.Count > 0)
                {
                    foreach (var item in daneSpotkania)
                    {
                        var dane = item.Split(';');
                        zapytanie = string.Format("INSERT INTO p_spotkania (data, liga, sezon, gosp, gosc, kurs_gosp, kurs_remis, kurs_gosc) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', {5}, {6}, {7})", dane[0], dane[1], dane[2], dane[3], dane[4], dane[5], dane[6], dane[7]);
                        komenda = new SqlCommand(zapytanie, polaczenie);
                        await komenda.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Nie ma nowych spotkań do dodania", "Brak spotkań", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę dodać spotkań do bazy.\n{0}", ex.Message);
                MessageBox.Show(byk, "Błąd dodawania spotkań", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
            }
            if (daneOstateczneSpotkania.Count > 0)
            {
                MessageBox.Show("Dodano spotkania do bazy danych", "Dodawanie spotkań", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void TypowanieNizej35Wyzej15()
        {
            status.Text = "Typuję wyniki...";
            try
            {
                if (polaczenie.State == ConnectionState.Closed)
                    polaczenie.Open();
                //string dzisiaj = DateTime.Today.ToString("yyyy-MM-dd");
                //string datado = DateTime.Today.AddDays(4).ToString("yyyy-MM-dd");
                zapytanie = "SELECT * FROM p_spotkania WHERE data BETWEEN convert(varchar, getdate(), 23) AND convert(varchar, getdate()+4, 23) ORDER BY data;";
                komenda = new SqlCommand(zapytanie, polaczenie);
                czytnik = await komenda.ExecuteReaderAsync();
                int ileSpotkan = 0, ileTypow = 0;
                int bgosp = 0, bgosc = 0, suma = 0, zgodne15 = 0, zgodne35 = 0, obie = 0, niestrzela = 0;
                if (czytnik.HasRows)
                {
                    while (czytnik.Read())
                    {
                        string data = czytnik.GetDateTime(0).ToString("yyyy-MM-dd");
                        string liga = czytnik.GetString(1);
                        string sezon = czytnik.GetString(2);
                        string gosp = czytnik.GetString(3);
                        string gosc = czytnik.GetString(4);
                        double kursGosp = czytnik.GetDouble(5);
                        double kursRemis = czytnik.GetDouble(6);
                        double kursGosc = czytnik.GetDouble(7);
                                                
                        gosp = gosp.Replace("'", "''");
                        gosc = gosc.Replace("'", "''");
                        podzapytanie = string.Format("SELECT count(*) FROM p_wyniki WHERE (gosp='{0}' AND gosc='{1}') OR (gosp='{2}' AND gosc='{3}')", gosp, gosc, gosc, gosp);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        ileSpotkan = Convert.ToInt32(await podkomenda.ExecuteScalarAsync());

                        podzapytanie = string.Format("SELECT gosp,gosc,wgosp,wgosc FROM p_wyniki WHERE (gosp='{0}' AND gosc='{1}') OR (gosp='{2}' AND gosc='{3}')", gosp, gosc, gosc, gosp);
                        podkomenda = new SqlCommand(podzapytanie, polaczenie);
                        podczytnik = await podkomenda.ExecuteReaderAsync();
                        if (podczytnik.HasRows && ileSpotkan > 2)
                        {
                            while (podczytnik.Read())
                            {
                                bgosp = podczytnik.GetInt16(2);
                                bgosc = podczytnik.GetInt16(3);
                                suma = bgosp + bgosc;
                                if (suma > 1)
                                {
                                    zgodne15++;
                                }
                                if (suma < 4)
                                {
                                    zgodne35++;
                                }
                                if (bgosp > 0 && bgosc > 0)
                                {
                                    obie++;
                                }
                                if ((bgosp > 0 && bgosc < 1) || (bgosc < 1 && bgosc > 0))
                                {
                                    niestrzela++;
                                }
                            }
                        }

                        if (ileSpotkan > 2 && zgodne15 > 2 && ileSpotkan == zgodne15)
                        {
                            richTextBox1.Text += data + ";" + liga + ";" + gosp + ";" + gosc + "; TYP: " + "Powyżej 1.5" + ";" + "\n";
                            ileTypow++;
                        }

                        if (ileSpotkan > 2 && zgodne35 > 2 && ileSpotkan == zgodne35)
                        {
                            //richTextBox1.Text += data + ";" + liga + ";" + gosp + ";" + gosc + "; TYP: " + "Poniżej 3.5" + ";" + "\n";
                            ileTypow++;
                        }
                        if (ileSpotkan > 2 && obie > 2 && ileSpotkan == obie)
                        {
                            //richTextBox1.Text += data + ";" + liga + ";" + gosp + ";" + gosc + "; TYP: " + "Obie strzelą" + ";" + "\n";
                            ileTypow++;
                        }
                        if (ileSpotkan > 2 && niestrzela > 2 && ileSpotkan == niestrzela)
                        {
                            //richTextBox1.Text += data + ";" + liga + ";" + gosp + ";" + gosc + "; TYP: " + "Nie strzelą obie" + ";" + "\n";
                            ileTypow++;
                        }
                        zgodne15 = 0;
                        zgodne35 = 0;
                        obie = 0;
                        niestrzela = 0;
                    }
                    if (ileTypow == 0)
                    {
                        MessageBox.Show("Brak typów", "Typy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                string byk = string.Format("Nie mogę wytypować 1.5 i 3.5.\n{0}", ex.Message);
                MessageBox.Show(byk, "Błąd usuwania", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                polaczenie.Close();
                status.Text = "Gotowe";
            }
        }
    }
}
