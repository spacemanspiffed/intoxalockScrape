using CsvHelper;
using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.ComponentModel;
using System.Linq;

namespace IntoxalockScrape
{
    class Program
    {
        private const string _fileLocation = "C:\\Scrapes\\firstTry.csv";
        static void Main(string[] args)
        {
            Console.WriteLine("Begin Scrape");
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load("https://www.intoxalock.com/locations/");

            var states = doc.DocumentNode.SelectNodes("//a[@class='Directory-listLink']");

            var states2 = states.Select(x => x.Attributes["href"].Value).Distinct().ToList();
            var competitors = new List<CompetitorAddress>();
            foreach (var state in states2)
            {
                //var stateLink = item.Attributes["href"].Value;

                Console.WriteLine("https://www.intoxalock.com/locations/" + state);

                HtmlDocument doc2 = web.Load("https://www.intoxalock.com/locations/" + state);

                var cities = doc2.DocumentNode.SelectNodes("//a[@class='Directory-listLink']");
                var cities2 = cities.Select(x => x.Attributes["href"].Value).Distinct().ToList();


                foreach (var city in cities2)
                {
                    var cityLink = cities.Where(x => x.Attributes["href"].Value == city).FirstOrDefault().Attributes["href"].Value;
                    //var cityLink = city.Attributes["href"].Value;
                    cityLink = cityLink.Replace("&#39;", "'");
                    HtmlDocument doc3 = web.Load($"https://www.intoxalock.com/locations/{cityLink}");

                    var teasers = doc3.DocumentNode.SelectNodes("//li[@class='Directory-listTeaser']");
                    if (teasers != null && teasers.Count > 0)
                    {
                        ExtractCompetitorFromTeaser(competitors, teasers);
                    }
                    else
                    {
                        var name = doc3.DocumentNode.SelectSingleNode("//span[@class='Hero-geo Heading--lead']");
                        var competitor = new CompetitorAddress();

                        competitor.Name = name.InnerText;
                        competitor.Address = doc3.DocumentNode.SelectSingleNode("//span[@class='c-address-street-1']").InnerText;
                        competitor.City = doc3.DocumentNode.SelectSingleNode("//span[@class='c-address-city']").InnerText;
                        competitor.State = state.ToUpper();
                        //doc3.DocumentNode.SelectSingleNode("//span[@class='c-address-state']").InnerText;
                        competitor.Zip = doc3.DocumentNode.SelectSingleNode("//span[@class='c-address-postal-code']").InnerText;
                        competitors.Add(competitor);
                    }

                    Console.WriteLine("Done With " + city);
                }
            }

            Console.WriteLine("Done");

            using (var writer = new StreamWriter(_fileLocation))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(competitors);
            }


            Console.ReadLine();

        }

        private static void ExtractCompetitorFromTeaser(List<CompetitorAddress> competitors, HtmlNodeCollection teasers)
        {


            foreach (var thing in teasers)
            {
                var crap = thing.Descendants();
                var competitor = new CompetitorAddress()
                {
                    Address = crap.Where(x => x.HasClass("c-address-street-1")).Select(x => x.InnerText).FirstOrDefault(),
                    Name = crap.Where(x => x.HasClass("LocationName-geo")).Select(x => x.InnerText).FirstOrDefault(),
                    City = crap.Where(x => x.HasClass("c-address-city")).Select(x => x.InnerText).FirstOrDefault(),
                    State = crap.Where(x => x.HasClass("c-address-state")).Select(x => x.InnerText).FirstOrDefault(),
                    Zip = crap.Where(x => x.HasClass("c-address-postal-code")).Select(x => x.InnerText).FirstOrDefault(),
                };
                Console.WriteLine(competitor.Address);

                competitors.Add(competitor);              
            }

            Console.WriteLine("Found this many " + competitors.Count.ToString());
        }

    }

    public class CompetitorAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }


}

