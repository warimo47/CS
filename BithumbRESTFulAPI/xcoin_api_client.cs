/*
 *
 * @brief XCoin API-call sample script (for C#)
 *
 * @author btckorea
 * @date 2017-04-14
 *
 * @details
 * This sample code uses the Newsoft.Json library for JSON decoding.
 * First, install Newtonsoft.Json with the following commands::
 * (if necessary)
 *
 * Open the console. "View" > "Other Windows" > "Package Manager Console"
 * Then type the following:
 * PM> Install-Package Newtonsoft.Json
 *
 * @note
 * Make sure current system time is correct.
 * If current system time is not correct, API request will not be processed normally.
 *
 */

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace XCoinAPIConsoleClient
{
    class XCoinAPI
    {
        private string m_sAPI_URL = "https://api.bithumb.com";
        private string m_sAPI_Key = "";
        private string m_sAPI_Secret = "";


        public XCoinAPI(string sAPI_Key, string sAPI_Secret)
        {
            this.m_sAPI_Key = sAPI_Key;
            this.m_sAPI_Secret = sAPI_Secret;
        }


        private string ByteToString(byte[] rgbyBuff)
        {
            string sHexStr = "";


            for (int nCnt = 0; nCnt < rgbyBuff.Length; nCnt++)
            {
                sHexStr += rgbyBuff[nCnt].ToString("x2"); // Hex format
            }

            return (sHexStr);
        }


        private byte[] StringToByte(string sStr)
        {
            byte[] rgbyBuff = Encoding.UTF8.GetBytes(sStr);

            return (rgbyBuff);
        }


        private long MicroSecTime()
        {
            long nEpochTicks = 0;
            long nUnixTimeStamp = 0;
            long nNowTicks = 0;
            long nowMiliseconds = 0;
            string sNonce = "";
            DateTime DateTimeNow;


            nEpochTicks = new DateTime(1970, 1, 1).Ticks;
            DateTimeNow = DateTime.UtcNow;
            nNowTicks = DateTimeNow.Ticks;
            nowMiliseconds = DateTimeNow.Millisecond;

            nUnixTimeStamp = ((nNowTicks - nEpochTicks) / TimeSpan.TicksPerSecond);

            sNonce = nUnixTimeStamp.ToString() + nowMiliseconds.ToString("D03");

            return (Convert.ToInt64(sNonce));
        }


        private string Hash_HMAC(string sKey, string sData)
        {
            byte[] rgbyKey = Encoding.UTF8.GetBytes(sKey);


            using (var hmacsha512 = new HMACSHA512(rgbyKey))
            {
                hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(sData));

                return (ByteToString(hmacsha512.Hash));
            }
        }


        public JObject xcoinApiCall(string sEndPoint, string sParams, ref string sRespBodyData)
        {
            string sAPI_Sign = "";
            string sPostData = sParams;
            string sHMAC_Key = "";
            string sHMAC_Data = "";
            string sResult = "";
            long nNonce = 0;
            HttpStatusCode nCode = 0;


            sPostData += "&endpoint=" + Uri.EscapeDataString(sEndPoint);

            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(this.m_sAPI_URL + sEndPoint);
                byte[] rgbyData = Encoding.ASCII.GetBytes(sPostData);


                nNonce = MicroSecTime();

                sHMAC_Key = this.m_sAPI_Secret;
                sHMAC_Data = sEndPoint + (char)0 + sPostData + (char)0 + nNonce.ToString();
                sResult = Hash_HMAC(sHMAC_Key, sHMAC_Data);
                sAPI_Sign = Convert.ToBase64String(StringToByte(sResult));

                Request.Headers.Add("Api-Key", this.m_sAPI_Key);
                Request.Headers.Add("Api-Sign", sAPI_Sign);
                Request.Headers.Add("Api-Nonce", nNonce.ToString());

                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded";
                Request.ContentLength = rgbyData.Length;

                using (var stream = Request.GetRequestStream())
                {
                    stream.Write(rgbyData, 0, rgbyData.Length);
                }

                var Response = (HttpWebResponse)Request.GetResponse();

                sRespBodyData = new StreamReader(Response.GetResponseStream()).ReadToEnd();

                return (JObject.Parse(sRespBodyData));
            }
            catch (WebException webEx)
            {
                using (HttpWebResponse Response = (HttpWebResponse)webEx.Response)
                {
                    nCode = Response.StatusCode;

                    using (StreamReader reader = new StreamReader(Response.GetResponseStream()))
                    {
                        sRespBodyData = reader.ReadToEnd();

                        return (JObject.Parse(sRespBodyData));
                    }
                }
            }

            return (null);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string sAPI_Key = File.ReadAllText(@"connectKey.txt");
            string sAPI_Secret = File.ReadAllText(@"secretKey.txt");

            string sParams = "order_currency=BTC&payment_currency=KRW";
            string sRespBodyData = String.Empty;
            XCoinAPI hAPI_Svr;
            JObject JObj = null;

            hAPI_Svr = new XCoinAPI(sAPI_Key, sAPI_Secret);

            //
            // public api
            //
            // /public/ticker
            // /public/recent_ticker
            // /public/orderbook
            // /public/recent_transactions

            Console.WriteLine("Bithumb Public API URI('/public/ticker') Request...");
            JObj = hAPI_Svr.xcoinApiCall("/public/ticker", sParams, ref sRespBodyData);
            if (JObj == null)
            {
                Console.WriteLine("Error occurred!");
                Console.WriteLine("HTTP Response JSON Data: {0}", sRespBodyData);
            }
            else
            {
                Console.WriteLine(JObj.ToString());

                if (String.Compare(JObj["status"].ToString(), "0000", true) == 0)
                {
                    Console.WriteLine("- Status Code: {0}", JObj["status"].ToString());
                    Console.WriteLine("- Opening Price: {0}", JObj["data"]["opening_price"].ToString());
                    Console.WriteLine("- Closing Price: {0}", JObj["data"]["closing_price"].ToString());
                    // Console.WriteLine("- Sell Price: {0}", JObj["data"]["sell_price"].ToString());
                    // Console.WriteLine("- Buy Price: {0}", JObj["data"]["buy_price"].ToString());
                }
            }

            Console.Write("\n\n");


            //
            // private api
            //
            // endpoint => parameters
            // /info/current
            // /info/account
            // /info/balance
            // /info/wallet_address

            /* Console.WriteLine("Bithumb Private API URI('/info/account') Request...");
            JObj = hAPI_Svr.xcoinApiCall("/info/account", sParams, ref sRespBodyData);
            if (JObj == null)
            {
                Console.WriteLine("Error occurred!");
                Console.WriteLine("HTTP Response JSON Data: {0}", sRespBodyData);
            }
            else {
                Console.WriteLine(JObj.ToString());

                if (String.Compare(JObj["status"].ToString(), "0000", true) == 0) {
                    Console.WriteLine("- Status Code: {0}", JObj["status"].ToString());
                    Console.WriteLine("- Created: {0}", JObj["data"]["created"].ToString());
                    Console.WriteLine("- Account ID: {0}", JObj["data"]["account_id"].ToString());
                    Console.WriteLine("- Trade Fee: {0}", JObj["data"]["trade_fee"].ToString());
                    Console.WriteLine("- Balance: {0}", JObj["data"]["balance"].ToString());
                }
            } */
        }
    }
}
