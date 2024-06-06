using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ZenekAdatbazis.Model;
using System.Diagnostics;

namespace ZenekAdatbazis
{
    internal class Program
    {
        static StreamReader file = new StreamReader("zenek.txt");
        static RadiokMusorai rm = new RadiokMusorai();
        static void Main(string[] args)
        {
            rm.Database.EnsureCreated(); //lényegében ez garantálja az adatbázis létrejöttét.

            AddRadioAllomasok();

            //a file-t olvasva feltölthetjük az előadókat is
            var eloadok_egyedi = new Dictionary<string, int>();
            var cimek_egyedi = new Dictionary<string, int>();
            ReadFiles(eloadok_egyedi, cimek_egyedi);

            AddEloadokAndSzamok(eloadok_egyedi, cimek_egyedi);

            file = new StreamReader("zenek.txt");
            long osszesSor = File.ReadLines("zenek.txt").Count();
            long beolvasottSor = 0;

            while (!file.EndOfStream)
            {
                string sor = file.ReadLine();
                string[] reszek = sor.Split(':');
                string[] radioReszek = reszek[0].Split(' ');

                int RadioId = int.Parse(radioReszek[0]);
                int perc = int.Parse(radioReszek[1]);
                int masodperc = int.Parse(radioReszek[2]);

                string eloadoNeve = radioReszek[3];
                for (int i = 4; i < radioReszek.Length; i++)
                {
                    eloadoNeve += (" " + radioReszek[i]);
                }
                string zeneCime = reszek[1];

                var eloado = rm.eloadok.SingleOrDefault(e => e.Nev == eloadoNeve);
                if (eloado == null)
                {
                    Console.WriteLine($"Előadó nem található: {eloadoNeve}");
                    continue;
                }
                var szam = rm.szamok.SingleOrDefault(s => s.Cim == zeneCime);
                if (szam == null)
                {
                    Console.WriteLine($"Zeneszám nem található: {zeneCime}");
                    continue;
                }

                Zene zene = new Zene()
                {
                    EloadoId = eloado.EloadoId,
                    SzamId = szam.SzamId,
                    Perc = perc,
                    Masodperc = masodperc,

                };
                rm.Add(zene);
                rm.SaveChanges();

                var ado = rm.adok.SingleOrDefault(a => a.RadioId == RadioId);
                if (ado == null)
                {
                    Console.WriteLine($"Rádióadó nem található: {RadioId}");
                    continue;
                }

                Musor musor = new Musor()
                {
                    RadioId = RadioId,
                    AdoRadioId = ado.RadioId,
                    ZeneId = zene.ZeneId,

                };
                rm.Add(musor);
                rm.SaveChanges();

                beolvasottSor++;
                CheckBack();

            }
            file.Close();
        }

        static void CheckBack()
        {
            Console.WriteLine("Az adatok beolvasása folyamatban van!");
        }

        static void AddRadioAllomasok()
        {
            //Rádióadók tekintetében a zenek.txt mindössze 1, 2, 3 kódértéket tartalmaz, megtehetjük, hogy azokat előre felvesszük
            Ado ado = new Ado(); //Ado elég egyszer
            ado.RadioId = 1;
            ado.Megnevezes = "Retro";
            rm.adok.Add(ado); //a rádiót, mint egy listához az adok táblájához hozzáadjuk
            ado = new Ado();
            ado.RadioId = 2;
            ado.Megnevezes = "Petőfi";
            rm.adok.Add(ado);
            ado = new Ado();
            ado.RadioId = 3;
            ado.Megnevezes = "Rádió 1";
            rm.adok.Add(ado);
            rm.SaveChanges(); //ez vezeti át ténylegesen a bővítést
        }

        static void ReadFiles(Dictionary<string, int> eloadok_egyedi, Dictionary<string, int> cimek_egyedi)
        {
            while (!file.EndOfStream)
            {
                string sor = file.ReadLine();
                string[] reszek = sor.Split(':');
                try
                {
                    cimek_egyedi[reszek[1]]++;
                }
                catch
                {
                    cimek_egyedi[reszek[1]] = 1;
                }
                reszek = reszek[0].Split(' ');
                string eloado_neve = reszek[3];
                for (int i = 4; i < reszek.Length; i++)
                {
                    eloado_neve += (" " + reszek[i]);
                }
                try
                {
                    eloadok_egyedi[eloado_neve]++;
                }
                catch
                {
                    eloadok_egyedi[eloado_neve] = 1;
                }
            }
            file.Close();
        }

        static void AddEloadokAndSzamok(Dictionary<string, int> eloadok_egyedi, Dictionary<string, int> cimek_egyedi)
        {
            foreach (KeyValuePair<string, int> e in eloadok_egyedi)
            {
                Eloado eloado = new Eloado();
                eloado.Nev = e.Key;
                rm.eloadok.Add(eloado);



            }
            foreach (KeyValuePair<string, int> e in cimek_egyedi)
            {
                Szam szam = new Szam();
                szam.Cim = e.Key;
                rm.szamok.Add(szam);
            }
            rm.SaveChanges();
        }

               
            
        
    }
}
