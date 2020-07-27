using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace WebApp.Controllers
{
    [Route("[controller]/[action]")]
    public class CurrencyController : ControllerBase
    {
        private readonly CurrencyContext Context;
        public CurrencyController(CurrencyContext context)
        {
            Context = context;
        }

        public bool UpdateCurrencies()
        {
            return UpdateRange(14);
        }

        private bool UpdateRange(int days)
        {
            for (var i = 0; i < days; i++)
                if (!UpdateCurrencyData(DateTime.Today.AddDays(-i))) return false;
            return true;
        }

        public IEnumerable<object> GetCurrencies()
        {
            return Context.Currency.Select(item => new
            {
                id = item.Id,
                name = ($"{item.Iso} - {(item.Nominal > 1 ? item.Nominal.ToString() + " " : "")}{item.Name}")
            })
                .ToArray();
        }

        public object GetRates(string id)
        {
            var cur = Context.Currency.Include("Rate").Where(item => item.Id == id).FirstOrDefault();
            if (cur != null)
            {
                return new
                {
                    name = cur.Iso,
                    dates = cur.Rate.Select(item => item.Date.ToString("dd MMM")).ToArray(),
                    values = cur.Rate.Select(item => item.Value).ToArray()
                };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }
        }
        private bool UpdateCurrencyData(DateTime date)
        {
            try
            {
                var ci = CultureInfo.GetCultureInfo("ru-RU");
                Currency c;
                Rate r;
                var xdoc = XDocument.Load($"http://www.cbr.ru/scripts/XML_daily.asp?date_req={date.ToString("dd'/'MM'/'yyyy")}");
                foreach (var item in xdoc.Descendants("Valute").Select(x => new
                {
                    Id = x.Attribute("ID").Value,
                    Name = x.Element("Name").Value,
                    ISO = x.Element("CharCode").Value,
                    Value = decimal.Parse(x.Element("Value").Value, ci),
                    Nominal = int.Parse(x.Element("Nominal").Value),
                }))
                {

                    c = Context.Currency.SingleOrDefault(e => e.Id == item.Id);
                    if (c != null)
                    {
                        c.Name = item.Name;
                        c.Nominal = item.Nominal;
                        c.Iso = item.ISO;
                    }
                    else
                        Context.Currency.Add(new Currency()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Nominal = item.Nominal,
                            Iso = item.ISO
                        });

                    r = Context.Rate.SingleOrDefault(e => e.CurrencyId == item.Id && e.Date == date);

                    if (r != null)
                    {
                        r.Value = item.Value;
                    }
                    else
                        Context.Rate.Add(new Rate()
                        {
                            CurrencyId = item.Id, //pk
                            Date = date, //pk
                            Value = item.Value
                        });
                }
                Context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //private bool UpdateCurrencyData(DateTime date)
        //{
        //    try
        //    {
        //        var ci = CultureInfo.GetCultureInfo("ru-RU");
        //        EntityState state;
        //        var xdoc = XDocument.Load($"http://www.cbr.ru/scripts/XML_daily.asp?date_req={date.ToString("dd'/'MM'/'yyyy")}");
        //        foreach (var item in xdoc.Descendants("Valute").Select(x => new
        //        {
        //            Id = x.Attribute("ID").Value,
        //            Name = x.Element("Name").Value,
        //            ISO = x.Element("CharCode").Value,
        //            Value = decimal.Parse(x.Element("Value").Value, ci),
        //            Nominal = int.Parse(x.Element("Nominal").Value),
        //        }))
        //        {
        //            state = Context.Currency.Any(e => e.Id == item.Id) ? EntityState.Modified : EntityState.Added;
        //            Context.Currency.Add(new Currency()
        //            {
        //                Id = item.Id, //pk
        //                Name = item.Name,
        //                Nominal = item.Nominal,
        //                Iso = item.ISO
        //            })
        //                .State = state;
        //            state = Context.Rate.Any(e => e.CurrencyId == item.Id && e.Date == date) ? EntityState.Modified : EntityState.Added;
        //            Context.Rate.Add(new Rate()
        //            {
        //                CurrencyId = item.Id, //pk
        //                Date = date, //pk
        //                Value = item.Value
        //            })
        //                .State = state;
        //        }
        //        Context.SaveChanges();
        //        return Context.Currency.Count() > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}


    }
}
