using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Data;
using System.Globalization;

namespace System.Doors.Data
{
    public static class TestDataBankStructure
    {
        public static void TestDataBankBuild()
        {               
            DataDeposit bvd00 = DataBank.Vault.Create("Tab1").NewDeposit("Dpt1");
            DataDeposit bvd01 = DataBank.Vault.Create("Tab2").NewDeposit("Dpt1");
            DataDeposit bvd02 = DataBank.Vault.Create("Tab3").NewDeposit("Dpt1");
            DataDeposit bvd03 = DataBank.Vault.Create("Tab4").NewDeposit("Dpt1");
            DataDeposit bvd10 = DataBank.Vault.Create("Tab5").NewDeposit("Dpt1");
            DataDeposit bvd11 = DataBank.Vault.Create("Tab6").NewDeposit("Dpt1");

            DataTrellis dt0 =  (DataTrellis)bvd00.SqlTrellis(testQry0());
            DataTrellis dt1 =  (DataTrellis)bvd01.SqlTrellis(testQry1());
            DataTrellis dt2 =  (DataTrellis)bvd02.SqlTrellis(testQry2());
            DataTrellis dt3 =  (DataTrellis)bvd03.SqlTrellis(testQry3());
            DataTrellis dt10 = (DataTrellis)bvd10.SqlTrellis(testQry10());
            DataTrellis dt11 = (DataTrellis)bvd11.SqlTrellis(testQry11());

            dt0.TrellName = "Table0"; dt1.TrellName = "Table1"; dt2.TrellName = "Table2"; dt3.TrellName = "Table3";
            dt10.TrellName = "Table10"; dt11.TrellName = "Table11";
        }

        public static string testQry0()
        {
            string dataQry = @"
                        SELECT           wbz.product_id, bfm.amz_product_id, bfm.amz_product_bufor_id, wbz.store_id, wbz.NazwaMagazyn, wbz.sku, wbz.ean, wbz.ASIN_SYS, 
                                         wbz.product_exts_id, wbz.model, wbz.upc, wbz.isbn, wbz.Nazwa, wbz.CenaZak, wbz.CenaMag, wbz.CenaInneMagAvg, wbz.Stawka, wbz.StanAktywny, 
                                         wbz.StanBlokada, wbz.Stan, wbz.StanInneMag, wbz.category_id, wbz.Asortyment, wbz.CenaSprzB, wbz.stock_type_id, wbz.stock_type_name, wbz.division_id, 
                                         wbz.storage_id, wbz.dok_zakup_ilosc, wbz.dok_zakup_cena, wbz.date_prod_modified, wbz.date_stock_modified, wbz.price_value_id_CenaZk, wbz.price_value_id_CenaAmz, 
                                         wbz.stock_status_id, wbz.active AS wbz_active, wbz.date_added, wbz.product_ext_id, bfm.ASIN, 
                                         bfm.ASIN_GL, bfm.NazwaAmz, bfm.Kolor, bfm.Rozmiar, bfm.CenaAmz, 
                                         bfm.NloscAmz, bfm.KategoriaAmz, bfm.RankCategory, bfm.search_status, bfm.search_status AS StatusAnalizy, 
                                         bfm.CenaMinProcPlan, (bfm.CenaMinProcPlan * 100) - 100 AS CenaMinProcPlanView, bfm.send_status AS StatusWysylki, 
                                         bfm.UrlAmz, bfm.small_image_url, bfm.CenaEwidPlan / bfm.kurs_waluty AS CenaEwidPlan, (bfm.CenaMinMagPlan / bfm.kurs_waluty) AS CenaMinMagPlan,
                                         bfm.best_price, bfm.marketplace_id, bfm.best_all_markets, bfm.Punktacja, CONVERT(decimal(18, 4), 100 * bfm.CenaStdProcPlan - 100) AS CenaStdProcPlanView,
                                         bfm.amazon_fee, bfm.kurs_waluty, bfm.tax_rate, 100 * bfm.CenaStdRabatPlan AS CenaStdRabatPlanView, 100 * bfm.CenaLowRabatPlan AS CenaLowRabatPlanView,
                                         bfm.amz_search_list_id, bfm.bestcount + bfm.onlycount AS bestoronlycount, bfm.tax_rate * 100 - 100 AS Vat,  bfm.CenaStdMagPlan / bfm.kurs_waluty AS CenaStdMagPlan,
                                         bfm.bestcount + bfm.onlycount + bfm.poorcount AS LiczbaKrajeZnalezione, bfm.bestcount, 
                                         bfm.onlycount, bfm.poorcount, bfm.nonecount, bfm.CenaMinPlan / bfm.kurs_waluty AS CenaMinPlan, bfm.KosztDostPlan / bfm.kurs_waluty AS KosztDostPlan,
                                         (bfm.MarzaKwotaPlan + bfm.CenaEwidPlan) / bfm.kurs_waluty AS CenaKalk, '' AS BIS#Market,  0.00 AS Min#MinCenaZDostAmz, 0.00 AS Max#MinCenaZDostAmz,
                                         0.00 AS Min#MinCenaAmz, 0.00 AS Max#MinCenaAmz, 0.00 AS Min#CenaPlan, 0.00 AS Max#CenaPlan, 0 AS SUM#CountFilterCountry, 0 AS SUM#NloscERP, 0 AS SUM#NloscAktywnaWbz, 
                                         0.00 AS Min#CenaMinPlan, 0.00 AS Max#CenaMinPlan, 0.00 AS Min#CenaKalk, 0.00 AS Max#CenaKalk, 0.00 AS Min#KosztPlanSum, 0.00 AS Max#KosztPlanSum, 0.00 AS Min#MarzaProcPlan, 
                                         0.00 AS Max#MarzaProcPlan, 0.00 AS Min#MarzaKwotaPlan, 0.00 AS Max#MarzaKwotaPlan, 0.00 AS Min#CenaRynkowaAmz, 0.00 AS Max#CenaRynkowaAmz, 0.00 AS Min#ProcRank, 0.00 AS Max#ProcRank, 
                                         0.00 AS Min#CenaLowRozPlanView, 0.00 AS Max#CenaLowRozPlanView, 0.00 AS Min#CenaStdRynkRozPlanView, 0.00 AS Max#CenaStdRynkRozPlanView, 0.00 AS Min#NarzutProcPlan, 
                                         0.00 AS Max#NarzutProcPlan, 0.00 AS Min#NarzutDodProcPlan, 0.00 AS Max#NarzutDodProcPlan, 0.00 AS Min#NarzutDodKwotaPlan, 0.00 AS Max#NarzutDodKwotaPlan, 0 AS Min#NloscOfert, 
                                         0 AS Max#NloscOfert, bfm.active, bfm.CountryFound, bfm.NloscOfert, ISNULL(bfm.updated, CONVERT(DATETIME, 
                                         '1901-01-01 00:00:00', 102)) AS updated, bfm.NloscSprzedaz, bfm.NloscSrDzien, bfm.PolitykaDniDoRab, bfm.PolitykaStart, bfm.PolitykaKoniec,
                                         bfm.CenaSrDzien / bfm.kurs_waluty AS CenaSrDzien, bfm.ZyskSrDzien / bfm.kurs_waluty AS ZyskSrDzien,  bfm.strategy_id,
                                         bfm.strategy_name, 0.00 AS SUM#ProgNlosc30, 0.00 AS SUM#ProgObrot30, 0.00 AS SUM#ProgZysk30, 0.00 AS Sum#PunktyMarket, bfm.buybox, 
                                         bfm.Waga, bfm.WagaJM, bfm.DataPublikacji
                        FROM             us_amz_product_bufor AS bfm INNER JOIN
                                         us_product_bufor AS wbz ON bfm.sku = wbz.sku              
                       WHERE             (bfm.main = 1)";

            return dataQry;

        }
        public static string testQry1()
        {

            string dataQry = @"
                                SELECT 
                                 wbzfm.product_id, bfm.amz_product_id, bfm.amz_product_bufor_id, bfm.store_id, bfm.sku, bfm.ean, 0.00 As Stan, bfm.ASIN, 
                                 bfm.ASIN_GL, bfm.NazwaAmz, bfm.Kolor, bfm.Rozmiar, 
                                 bfm.CenaAmz / bfm.kurs_waluty AS CenaAmz, bfm.NloscAmz, bfm.KategoriaAmz, 
                                 bfm.CenaRynkowaAmz / bfm.kurs_waluty AS CenaRynkowaAmz, 
                                 bfm.MinCenaZDostAmz / bfm.kurs_waluty AS MinCenaZDostAmz, bfm.MinCenaAmz / bfm.kurs_waluty AS MinCenaAmz, 
                                 bfm.MinDostAmz / bfm.kurs_waluty AS MinDostAmz, bfm.RankCategory, bfm.search_status AS StatusAnalizy, 
                                 bfm.send_status AS StatusWysylki, bfm.KosztDostPlan / bfm.kurs_waluty AS KosztDostPlan, 
                                 bfm.CenaDostPlan / bfm.kurs_waluty AS CenaDostPlan, bfm.ProwizjaPlan / bfm.kurs_waluty AS ProwizjaPlan, 
                                 bfm.KosztDostAmz / bfm.kurs_waluty AS KosztDostAmz, bfm.CenaDostAmz / bfm.kurs_waluty AS CenaDostAmz, 
                                 bfm.ProwizjaAmz / bfm.kurs_waluty AS ProwizjaAmz, bfm.CenaMinPlan / bfm.kurs_waluty AS CenaMinPlan, 
                                 bfm.CenaMinAmz / bfm.kurs_waluty AS CenaMinAmz, bfm.CenaPlan / bfm.kurs_waluty AS CenaPlan, 
                                 bfm.MarzaProcPlan, bfm.MarzaKwotaPlan / bfm.kurs_waluty AS MarzaKwotaPlan, bfm.Waluta, 
                                 bfm.MarzaProcAmz, bfm.MarzaKwotaAmz, bfm.KosztPlan / bfm.kurs_waluty AS KosztPlan, bfm.KosztAmz, bfm.UrlAmz, 
                                 bfm.small_image_url, bfm.CenaEwidPlan / bfm.kurs_waluty AS CenaEwidPlan, bfm.CenaFeedAmz / bfm.kurs_waluty AS CenaFeedAmz, 
                                 bfm.CenaBezDost / bfm.kurs_waluty AS CenaBezDost, bfm.CenaZDostAmz / bfm.kurs_waluty AS CenaZDostAmz, 
                                 bfm.CenaMinMagPlan / bfm.kurs_waluty AS CenaMinMagPlan, bfm.CenaStdMagPlan / bfm.kurs_waluty AS CenaStdMagPlan, 
                                 bfm.CenaLowMagPlan / bfm.kurs_waluty AS CenaLowMagPlan, 
                                 bfm.CenaRynkowaMagPlan / bfm.kurs_waluty AS CenaRynkowaMagPlan, bfm.CenaMinProcPlan, CONVERT(decimal(18, 4), 
                                 100 * bfm.CenaMinProcPlan - 100) AS CenaMinProcPlanView, bfm.CenaStdProcPlan, bfm.CenaStdPlan / bfm.kurs_waluty AS CenaStdPlan, CONVERT(decimal(18, 4), 100 * bfm.CenaStdProcPlan - 100) 
                                 AS CenaStdProcPlanView, bfm.CenaLowProcPlan, bfm.CenaRynkowaProcPlan, bfm.CenaStdRozPlan, bfm.CenaLowRozPlan, 
                                 100 * bfm.CenaLowRozPlan AS CenaLowRozPlanView, bfm.CenaStdRynkRozPlan, 100 * bfm.CenaStdRynkRozPlan AS CenaStdRynkRozPlanView, 
                                 bfm.CenaRynkowaRozPlan, bfm.CenaStdRabatPlan, 100 * bfm.CenaStdRabatPlan AS CenaStdRabatPlanView, 
                                 bfm.CenaLowRabatPlan, 100 * bfm.CenaLowRabatPlan AS CenaLowRabatPlanView, bfm.CenaLowDefDiffPlan, 
                                 bfm.KosztDostPlan / bfm.tax_rate / bfm.kurs_waluty AS KosztDostNetPlan, (bfm.KosztPlan + bfm.PodatekPlan) / bfm.kurs_waluty AS KosztPlanSum, 
                                 bfm.CenaPrzedRabatPlan / bfm.kurs_waluty AS CenaPrzedRabatPlan, bfm.amz_fee_proc, bfm.wbz_fee_proc, bfm.wbz_fee_penalty_proc,
                                 bfm.CenaRabatPlan / bfm.kurs_waluty AS CenaRabatPlan, 100 * bfm.CenaRabatPlan AS CenaRabatPlanView, 
                                 bfm.MarzaDodProcPlan, CONVERT(decimal(10, 4), bfm.Ranking * 100 / bfm.MaxRanking) AS ProcRank, bfm.MaxRanking, 
                                 bfm.Ranking, bfm.rank_status AS StatusRankingu, bfm.price_status AS StatusCeny, bfm.best_price, 
                                 bfm.marketplace_id, bfm.best_all_markets, bfm.PunktyMarket, bfm.Punktacja, bfm.amazon_fee, bfm.kurs_waluty, 
                                 bfm.tax_rate, bfm.std_price, bfm.bestcount + bfm.onlycount AS bestoronlycount, 
                                 bfm.tax_rate * 100 - 100 AS Vat, bfm.bestcount + bfm.onlycount + bfm.poorcount AS LiczbaKrajeZnalezione, 
                                 bfm.bestcount, bfm.onlycount, bfm.poorcount, bfm.nonecount, 
                                 (bfm.MarzaKwotaPlan + bfm.CenaEwidPlan) / bfm.kurs_waluty AS CenaKalk, '' AS BIS#Market, bfm.active, 
                                 bfm.NloscOfert, ISNULL(bfm.updated, CONVERT(DATETIME, 
                                 '1901-01-01 00:00:00', 102)) AS updated, bfm.NloscSprzedaz, bfm.NloscSrDzien / bfm.kurs_waluty AS NloscSrDzien, 
                                 bfm.CenaSrDzien / bfm.kurs_waluty AS CenaSrSDzien, bfm.ProgNloscSrDzien, bfm.NarzutProcPlan, 
                                 bfm.NarzutDodProcPlan, bfm.NarzutDodKwotaPlan / bfm.kurs_waluty AS NarzutDodKwotaPlan, bfm.DataOstSprzedaz, 
                                 bfm.DataOstAukcji, bfm.PolitykaDniDoRab, bfm.PolitykaStart, bfm.PolitykaKoniec, bfm.strategy_id, 
                                 bfm.strategy_name, bfm.PodatekPlan / bfm.kurs_waluty AS PodatekPlan, bfm.search_status, bfm.amz_search_list_id, 
                                 'flag-' + bfm.market_name AS Market, 1 AS CountFilterCountry, ISNULL(NULLIF (bfm.CenaAmz, 0) * 0 + 1, 0) AS CountMarketSend, 
                                 bfm.buybox, bfm.ProgNloscSrDzien * 30 AS ProgNlosc30, bfm.ProgNloscSrDzien * 30 * bfm.CenaPlan / bfm.kurs_waluty AS ProgObrot30, 
                                 bfm.ProgNloscSrDzien * 30 * bfm.MarzaKwotaPlan / bfm.kurs_waluty AS ProgZysk30
                      FROM       us_amz_product_bufor AS bfm INNER JOIN
                                 us_product_bufor as wbzfm ON bfm.sku = wbzfm.sku         
                        WHERE    (bfm.search_status Like '%ok%')";

            return dataQry;

        }
        public static string testQry2()
        {
            string dataQry = @"
               SELECT        PS.product_id, PS.stock_id, PS.store_id, PS.division_id, PS.storage_id, D.name AS division_name, S.name AS storage_name, PS.ilosc AS NloscERP, PS.ilosc_rezerw, PS.cenazk, NULLIF (PS.data_zakup, 
                         CONVERT(DATETIME, '1900-01-01 00:00:00', 102)) AS data_ost_zakup, NULLIF (PS.dok_zakup_ilosc, 0) AS ilosc_ost_zakup, PS.dok_zakup AS dok_ost_zakup, ISNULL(PS.data_modyfikacji, CONVERT(DATETIME, 
                         '1901-01-01 00:00:00', 102)) AS updated
                FROM            us_product_stock AS PS INNER JOIN
                                            us_storage AS S ON PS.storage_id = S.storage_id INNER JOIN
                                            us_division AS D ON PS.division_id = D.division_id INNER JOIN
                                            us_product ON PS.product_id = us_product.product_id
                WHERE        (S.[default] = 0)
                ORDER BY PS.stock_id
                ";

            return dataQry;

        }
        public static string testQry3()
        {
            string dataQry = @"
                      SELECT        PS.product_id, PS.stock_id, PS.store_id, PS.division_id, PS.storage_id,  D.name AS division_name, S.name AS storage_name, PS.ilosc - PS.ilosc_rezerw AS NloscAktywnaWbz, PS.ilosc_rezerw AS NloscRezerWbz,
                          PS.ilosc AS NloscWbz, PS.cenazk, NULLIF (PS.data_zakup, CONVERT(DATETIME, '1900-01-01 00:00:00', 102)) AS data_ost_zakup, NULLIF (PS.dok_zakup_ilosc, 0) AS ilosc_ost_zakup, 
                         PS.dok_zakup AS dok_ost_zakup, ISNULL(PS.data_modyfikacji, CONVERT(DATETIME, 
                         '1901-01-01 00:00:00', 102)) AS updated
                      FROM            us_product_stock AS PS INNER JOIN
                         us_storage AS S ON PS.storage_id = S.storage_id INNER JOIN
                         us_division AS D ON PS.division_id = D.division_id INNER JOIN
                         us_product ON PS.product_id = us_product.product_id
                      WHERE        (S.[default] = 1) 
                ORDER BY PS.stock_id
                ";
            return dataQry;

        }
        public static string testQry10()
        {
            string dataQry = @"
        SELECT amz_product_bufor_list_id, amz_product_id, 101010101 AS product_id, sku, store_id, ean, Nazwa, Stan, CenaZak, CenaMinProcPlan, (CenaMinProcPlan * 100) - 100 AS CenaMinProcPlanView, CenaMinMagPlan / kurs_waluty AS CenaMinMagPlan, type, ASIN_SYS, ASIN,
                              ASIN_GL, NazwaAmz, Kolor, Rozmiar, CenaAmz, NloscAmz, KategoriaAmz, search_status, search_status AS StatusAnalizy, UrlAmz, small_image_url, CenaEwidPlan / kurs_waluty as CenaEwidPlan, marketplace_id, best_all_markets, Punktacja, amazon_fee, kurs_waluty,
                              tax_rate, std_price, amz_search_list_id, bestcount + onlycount AS bestoronlycount, tax_rate * 100 - 100 AS VAT, bestcount + onlycount + poorcount AS LiczbaKrajeZnalezione, bestcount, onlycount, poorcount, nonecount, CenaStdMagPlan / kurs_waluty AS CenaStdMagPlan,
                              CONVERT(decimal(18, 4), 100 * CenaStdProcPlan - 100) AS CenaStdProcPlanView, 100 * CenaLowRabatPlan AS CenaLowRabatPlanView, 100 * CenaStdRabatPlan AS CenaStdRabatPlanView, 0.00 AS Min#MinCenaAmz, 0.00 AS Max#MinCenaAmz,
                                     (MarzaKwotaPlan + CenaEwidPlan) / kurs_waluty AS CenaKalk, '' AS BIS#Flag, 0.00 AS Min#MinCenaZDostAmz, 0.00 AS Max#MinCenaZDostAmz, 0.00 AS Min#CenaPlan, 0.00 AS Max#CenaPlan, CenaMinPlan / kurs_waluty AS CenaMinPlan, KosztDostPlan / kurs_waluty AS KosztDostPlan,
                                     0 AS SUM#CountFilterCountry, 0.00 AS Min#CenaMinPlan, 0.00 AS Max#CenaMinPlan, 0.00 AS Min#CenaKalk, 0.00 AS Max#CenaKalk, 0.00 AS Min#KosztPlanSum, 0.00 AS Max#KosztPlanSum, 0.00 AS Sum#PunktyMarket, 
                                     0.00 AS Min#MarzaProcPlan, 0.00 AS Max#MarzaProcPlan, 0.00 AS Min#MarzaKwotaPlan, 0.00 AS Max#MarzaKwotaPlan, 0.00 AS Min#CenaRynkowaAmz, 0.00 AS Max#CenaRynkowaAmz, 
                                     0.00 AS Min#ProcRank, 0.00 AS Max#ProcRank, 0.00 AS Min#CenaLowRozPlanView, 0.00 AS Max#CenaLowRozPlanView, 0.00 AS Min#CenaStdRynkRozPlanView, 0.00 AS Max#CenaStdRynkRozPlanView, 
                                     0.00 AS Min#NarzutProcPlan, 0.00 AS Max#NarzutProcPlan, 0 AS Min#NloscOfert, 0 AS Max#NloscOfert, active, CountryFound, NloscOfert, ISNULL(updated, CONVERT(DATETIME, '1901-01-01 00:00:00', 102)) 
                                     AS updated, 0.00 AS SUM#ProgNlosc30, 0.00 AS SUM#ProgObrot30, 0.00 AS SUM#ProgZysk30, CenaSrDzien, ZyskSrDzien, Waga, WagaJM, 
                                     DataPublikacji, 0.00 AS Min#NarzutDodProcPlan, 0.00 AS Max#NarzutDodProcPlan, 0.00 AS Min#NarzutDodKwotaPlan, 0.00 AS Max#NarzutDodKwotaPlan
                        FROM         us_amz_product_bufor_list AS us_amz_product_bufor_1

                 WHERE(main = 1)
                     ";
            return dataQry;

        }
        public static string testQry11()
        {
            string dataQry = @"
                        SELECT       amz_product_bufor_list_id, amz_product_id, 101010101 AS product_id, sku, store_id, ean, Nazwa, Stan, CenaZak, [type], ASIN_SYS, 'Brak' AS Producent, 'Brak' AS Parametr_1, 'Brak' AS Parametr_2, '0' AS CenaSprz, ASIN, ASIN_GL, NazwaAmz, 
                                     Kolor, Rozmiar, CenaAmz, NloscAmz, KategoriaAmz, CenaRynkowaAmz, MinCenaZDostAmz / kurs_waluty AS MinCenaZDostAmz, MinCenaAmz / kurs_waluty AS MinCenaAmz, MinDostAmz, RankCategory,  search_status, search_status AS StatusAnalizy, 
                                     send_status AS StatusWysylki, KosztDostPlan / kurs_waluty AS KosztDostPlan, CenaDostPlan / kurs_waluty AS CenaDostPlan, ProwizjaPlan / kurs_waluty AS ProwizjaPlan, 
                                     KosztDostAmz / kurs_waluty AS KosztDostAmz, CenaDostAmz / kurs_waluty AS CenaDostAmz, ProwizjaAmz / kurs_waluty AS ProwizjaAmz, CenaMinPlan / kurs_waluty AS CenaMinPlan, 
                                     CenaMinAmz / kurs_waluty AS CenaMinAmz, CenaPlan / kurs_waluty AS CenaPlan, MarzaProcPlan, NarzutProcPlan, MarzaKwotaPlan / kurs_waluty AS MarzaKwotaPlan, 'PLN' AS PLN, Waluta, MarzaProcAmz, 
                                     MarzaKwotaAmz / kurs_waluty AS MarzaKwotaAmz, KosztPlan / kurs_waluty AS KosztPlan, KosztAmz / kurs_waluty AS KosztAmz, UrlAmz, small_image_url, CenaEwidPlan / kurs_waluty AS CenaEwidPlan, 
                                     CenaFeedAmz / kurs_waluty AS CenaFeedAmz, CenaBezDost / kurs_waluty AS CenaBezDost, CenaZDostAmz / kurs_waluty AS CenaZDostAmz, CenaMinMagPlan / kurs_waluty AS CenaMinMagPlan, 
                                     CenaStdMagPlan / kurs_waluty AS CenaStdMagPlan, CenaLowMagPlan / kurs_waluty AS CenaLowMagPlan, CenaRynkowaMagPlan / kurs_waluty AS CenaRynkowaMagPlan, CenaMinProcPlan, 
                                     CONVERT(decimal(18, 4), 100 * CenaMinProcPlan - 100) AS CenaMinProcPlanView, CenaStdProcPlan, CenaStdPlan / kurs_waluty AS CenaStdPlan, CONVERT(decimal(18, 4), 100 * CenaStdProcPlan - 100) AS CenaStdProcPlanView, CenaLowProcPlan, 
                                     CenaRynkowaProcPlan, CenaStdRozPlan, CenaLowRozPlan, 100 * CenaLowRozPlan AS CenaLowRozPlanView, CenaStdRynkRozPlan, 100 * CenaStdRynkRozPlan AS CenaStdRynkRozPlanView, 
                                     CenaRynkowaRozPlan, CenaStdRabatPlan, 100 * CenaStdRabatPlan AS CenaStdRabatPlanView, CenaLowRabatPlan, 100 * CenaLowRabatPlan AS CenaLowRabatPlanView, CenaLowDefDiffPlan, 
                                     KosztDostPlan / tax_rate / kurs_waluty AS KosztDostNetPlan, (KosztPlan + PodatekPlan) / kurs_waluty AS KosztPlanSum, PodatekPlan / kurs_waluty AS PodatekPlan, 
                                     CenaPrzedRabatPlan / kurs_waluty AS CenaPrzedRabatPlan, amz_fee_proc, wbz_fee_proc, wbz_fee_penalty_proc,
                                     CenaRabatPlan / kurs_waluty AS CenaRabatPlan, 100 * CenaRabatPlan AS CenaRabatPlanView, MarzaDodProcPlan, NarzutDodProcPlan, NarzutDodKwotaPlan / kurs_waluty AS NarzutDodKwotaPlan, 
                                     CONVERT(decimal(10, 4), Ranking * 100 / MaxRanking) AS ProcRank, MaxRanking, Ranking, rank_status AS StatusRankingu, price_status AS StatusCeny, best_price, marketplace_id, best_all_markets, PunktyMarket, Punktacja, 
                                     market_name AS Market, amazon_fee, kurs_waluty, tax_rate, std_price, amz_search_list_id, tax_rate * 100 - 100 AS VAT, bestcount + onlycount + poorcount AS LiczbaKrajeZnalezione, bestcount, onlycount, 
                                     poorcount, nonecount, (MarzaKwotaPlan + CenaEwidPlan) / kurs_waluty AS CenaKalk, 1 AS CountFilterCountry, 'flag-' + market_name AS Flag, active, NloscOfert, ISNULL(updated, CONVERT(DATETIME, 
                                     '1901-01-01 00:00:00', 102)) AS updated, DataPublikacji, ProgNloscSrDzien * 30 AS ProgNlosc30, (ProgNloscSrDzien * 30 * CenaPlan) / kurs_waluty AS ProgObrot30, (ProgNloscSrDzien * 30 * MarzaKwotaPlan) / kurs_waluty AS ProgZysk30
                        FROM         us_amz_product_bufor_list AS us_amz_product_bufor_1
                        WHERE        (search_status LNKE 'ok%') 
                    ";
            return dataQry;

        }
    }   
}
