using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Doors.Data;

namespace System.Doors.Data
{

    public class TestBinarySerializaion
    {
        //public static void TestSerialization()
        //{
        //    DataDeposit bvd00 = DataBank.Vault.Create();
        //    NTable itab0 = (NTable)bvd00.Call.SqlToNTable(testQry0()).Prime;
        //    itab0.PrimaryKey = itab0.NColumns.GetNColumns(new List<string>() { "amz_product_id", "product_id" });
        //    RawMattab nvr = new RawMattab();
        //    MemoryStream rms = new MemoryStream();
        //    nvr.Serialize(rms, itab0.NRows.Select(ia => ia.Cells.CellValues.ToArray()).ToList());
        //    rms.Position = 0;
        //    object[][] nnew = nvr.Deserialize(rms, itab0.NColumns.List.Select(d => d.DataType).ToArray());
        //    RawGeneric ndb = new RawGeneric();
        //    MemoryStream bms = new MemoryStream();
        //    ndb.Serialize(bms, itab0);
        //    NTable ntab = (NTable)ndb.Deserialize(bms);
        //}

        public static string testQry0()
        {
            string dataQry = @"
                        SELECT           wbz.product_id, bfm.amz_product_id, bfm.amz_product_bufor_id, wbz.store_id, wbz.NazwaMagazyn, wbz.sku, wbz.ean, wbz.ASIN_SYS, 
                                         wbz.product_exts_id, wbz.model, wbz.upc, wbz.isbn, wbz.Nazwa, wbz.CenaZak, wbz.CenaMag, wbz.CenaInneMagAvg, wbz.Stawka, wbz.StanAktywny, 
                                         wbz.StanBlokada, wbz.Stan, wbz.StanInneMag, wbz.category_id, wbz.Asortyment, wbz.CenaSprzB, wbz.stock_type_id, wbz.stock_type_name, wbz.division_id, 
                                         wbz.storage_id, wbz.dok_zakup_ilosc, wbz.dok_zakup_cena, wbz.date_prod_modified, wbz.date_stock_modified, wbz.price_value_id_CenaZk, wbz.price_value_id_CenaAmz, 
                                         wbz.stock_status_id, wbz.active AS wbz_active, wbz.date_added, wbz.product_ext_id, bfm.ASIN, 
                                         bfm.ASIN_GL, bfm.NazwaAmz, bfm.Kolor, bfm.Rozmiar, bfm.CenaAmz, 
                                         bfm.IloscAmz, bfm.KategoriaAmz, bfm.RankCategory, bfm.search_status, bfm.search_status AS StatusAnalizy, 
                                         bfm.CenaMinProcPlan, (bfm.CenaMinProcPlan * 100) - 100 AS CenaMinProcPlanView, bfm.send_status AS StatusWysylki, 
                                         bfm.UrlAmz, bfm.small_image_url, bfm.CenaEwidPlan / bfm.kurs_waluty AS CenaEwidPlan, (bfm.CenaMinMagPlan / bfm.kurs_waluty) AS CenaMinMagPlan,
                                         bfm.best_price, bfm.marketplace_id, bfm.best_all_markets, bfm.Punktacja, CONVERT(decimal(18, 4), 100 * bfm.CenaStdProcPlan - 100) AS CenaStdProcPlanView,
                                         bfm.amazon_fee, bfm.kurs_waluty, bfm.tax_rate, 100 * bfm.CenaStdRabatPlan AS CenaStdRabatPlanView, 100 * bfm.CenaLowRabatPlan AS CenaLowRabatPlanView,
                                         bfm.amz_search_list_id, bfm.bestcount + bfm.onlycount AS bestoronlycount, bfm.tax_rate * 100 - 100 AS Vat,  bfm.CenaStdMagPlan / bfm.kurs_waluty AS CenaStdMagPlan,
                                         bfm.bestcount + bfm.onlycount + bfm.poorcount AS LiczbaKrajeZnalezione, bfm.bestcount, 
                                         bfm.onlycount, bfm.poorcount, bfm.nonecount, bfm.CenaMinPlan / bfm.kurs_waluty AS CenaMinPlan, bfm.KosztDostPlan / bfm.kurs_waluty AS KosztDostPlan,
                                         (bfm.MarzaKwotaPlan + bfm.CenaEwidPlan) / bfm.kurs_waluty AS CenaKalk, '' AS BNS#Market,  0.00 AS Min#MinCenaZDostAmz, 0.00 AS Max#MinCenaZDostAmz,
                                         0.00 AS Min#MinCenaAmz, 0.00 AS Max#MinCenaAmz, 0.00 AS Min#CenaPlan, 0.00 AS Max#CenaPlan, 0 AS SUM#CountFilterCountry, 0 AS SUM#IloscERP, 0 AS SUM#IloscAktywnaWbz, 
                                         0.00 AS Min#CenaMinPlan, 0.00 AS Max#CenaMinPlan, 0.00 AS Min#CenaKalk, 0.00 AS Max#CenaKalk, 0.00 AS Min#KosztPlanSum, 0.00 AS Max#KosztPlanSum, 0.00 AS Min#MarzaProcPlan, 
                                         0.00 AS Max#MarzaProcPlan, 0.00 AS Min#MarzaKwotaPlan, 0.00 AS Max#MarzaKwotaPlan, 0.00 AS Min#CenaRynkowaAmz, 0.00 AS Max#CenaRynkowaAmz, 0.00 AS Min#ProcRank, 0.00 AS Max#ProcRank, 
                                         0.00 AS Min#CenaLowRozPlanView, 0.00 AS Max#CenaLowRozPlanView, 0.00 AS Min#CenaStdRynkRozPlanView, 0.00 AS Max#CenaStdRynkRozPlanView, 0.00 AS Min#NarzutProcPlan, 
                                         0.00 AS Max#NarzutProcPlan, 0.00 AS Min#NarzutDodProcPlan, 0.00 AS Max#NarzutDodProcPlan, 0.00 AS Min#NarzutDodKwotaPlan, 0.00 AS Max#NarzutDodKwotaPlan, 0 AS Min#IloscOfert, 
                                         0 AS Max#IloscOfert, bfm.active, bfm.CountryFound, bfm.IloscOfert, ISNULL(bfm.updated, CONVERT(DATETIME, 
                                         '1901-01-01 00:00:00', 102)) AS updated, bfm.IloscSprzedaz, bfm.IloscSrDzien, bfm.PolitykaDniDoRab, bfm.PolitykaStart, bfm.PolitykaKoniec,
                                         bfm.CenaSrDzien / bfm.kurs_waluty AS CenaSrDzien, bfm.ZyskSrDzien / bfm.kurs_waluty AS ZyskSrDzien,  bfm.strategy_id,
                                         bfm.strategy_name, 0.00 AS SUM#ProgIlosc30, 0.00 AS SUM#ProgObrot30, 0.00 AS SUM#ProgZysk30, 0.00 AS Sum#PunktyMarket, bfm.buybox, 
                                         bfm.Waga, bfm.WagaJM, bfm.DataPublikacji
                        FROM             us_amz_product_bufor AS bfm INNER JOIN
                                         us_product_bufor AS wbz ON bfm.sku = wbz.sku              
                       WHERE             (bfm.main = 1)";

            return dataQry;

        }
    }
}
