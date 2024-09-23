using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietQRHelper
{
    public class QRPay
    {
        public bool isValid = true;
        public string version;
        public string initMethod;
        public Provider provider;
        public Merchant merchant;
        public Consumer consumer;
        public string category;
        public string currency;
        public string amount;
        public string tipAndFeeType;
        public string tipAndFeeAmount;
        public string tipAndFeePercent;
        public string nation;
        public string city;
        public string zipCode;
        public AdditionalData additionalData;
        public string crc;
        public Dictionary<string, string> EVMCo;
        public Dictionary<string, string> unreserved;

        public QRPay(string content = "")
        {
            provider = new Provider();
            consumer = new Consumer();
            merchant = new Merchant();
            additionalData = new AdditionalData();
            parse(content ?? "");
        }

        private bool invalid()
        {
            this.isValid = false;
            return this.isValid;
        }
        public static bool VerifyCRC(string content)
        {
            string checkContent = content.Substring(0, content.Length - 4);
            string crcCode = content.Substring(content.Length - 4).ToUpper();

            string genCrcCode = QRPay.GenCRCCode(checkContent);
            return crcCode == genCrcCode;
        }

        public static string GenCRCCode(string content)
        {
            int crcCode = CRC16.crc16ccitt(content);
            return $"0000{crcCode:X}".Substring(Math.Max(0, $"0000{crcCode:X}".Length - 4));
        }

        private static (string id, int length, string value, string nextValue) SliceContent(string content)
        {
            string id = content.Substring(0, 2);
            int length = int.Parse(content.Substring(2, 2));
            string value = content.Substring(4, length);
            string nextValue = content.Substring(4 + length);
            return (id, length, value, nextValue);
        }

        private void ParseProviderInfo(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            switch (id)
            {
                case ProviderFieldID.GUID:
                    this.provider.guid = value;
                    break;
                case ProviderFieldID.DATA:
                    if (this.provider.guid == QRProviderGUID.VNPAY)
                    {
                        this.provider.name = QRProvider.VNPAY;
                        this.merchant.id = value;
                    }
                    else if (this.provider.guid == QRProviderGUID.VIETQR)
                    {
                        this.provider.name = QRProvider.VIETQR;
                        this.ParseVietQRConsumer(value);
                    }
                    break;
                case ProviderFieldID.SERVICE:
                    this.provider.service = value;
                    break;
                default:
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseProviderInfo(nextValue);
        }

        private void ParseVietQRConsumer(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            switch (id)
            {
                case VietQRConsumerFieldID.BANK_BIN:
                    this.consumer.bankBin = value;
                    break;
                case VietQRConsumerFieldID.BANK_NUMBER:
                    this.consumer.bankNumber = value;
                    break;
                default:
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseVietQRConsumer(nextValue);
        }

        private void ParseAdditionalData(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            switch (id)
            {
                case AdditionalDataID.PURPOSE_OF_TRANSACTION:
                    this.additionalData.purpose = value;
                    break;
                case AdditionalDataID.BILL_NUMBER:
                    this.additionalData.billNumber = value;
                    break;
                case AdditionalDataID.MOBILE_NUMBER:
                    this.additionalData.mobileNumber = value;
                    break;
                case AdditionalDataID.REFERENCE_LABEL:
                    this.additionalData.reference = value;
                    break;
                case AdditionalDataID.STORE_LABEL:
                    this.additionalData.store = value;
                    break;
                case AdditionalDataID.TERMINAL_LABEL:
                    this.additionalData.terminal = value;
                    break;
                default:
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseAdditionalData(nextValue);
        }

        private bool ParseRootContent(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            if (value.Length != length)
                return invalid();
            switch (id)
            {
                case FieldID.VERSION:
                    this.version = value;
                    break;
                case FieldID.INIT_METHOD:
                    this.initMethod = value;
                    break;
                case FieldID.VIETQR:
                case FieldID.VNPAYQR:
                    this.provider.fieldId = id;
                    this.ParseProviderInfo(value);
                    break;
                case FieldID.CATEGORY:
                    this.category = value;
                    break;
                case FieldID.CURRENCY:
                    this.currency = value;
                    break;
                case FieldID.AMOUNT:
                    this.amount = value;
                    break;
                case FieldID.TIP_AND_FEE_TYPE:
                    this.tipAndFeeType = value;
                    break;
                case FieldID.TIP_AND_FEE_AMOUNT:
                    this.tipAndFeeAmount = value;
                    break;
                case FieldID.TIP_AND_FEE_PERCENT:
                    this.tipAndFeePercent = value;
                    break;
                case FieldID.NATION:
                    this.nation = value;
                    break;
                case FieldID.MERCHANT_NAME:
                    this.merchant.name = value;
                    break;
                case FieldID.CITY:
                    this.city = value;
                    break;
                case FieldID.ZIP_CODE:
                    this.zipCode = value;
                    break;
                case FieldID.ADDITIONAL_DATA:
                    this.ParseAdditionalData(value);
                    break;
                case FieldID.CRC:
                    this.crc = value;
                    break;
                default:
                    var idNum = int.Parse(id);
                    if (idNum >= 65 && idNum <= 79)
                    {
                        if (this.EVMCo == null)
                            this.EVMCo = new Dictionary<string, string>();
                        this.EVMCo[id] = value;
                    }
                    else if (idNum >= 80 && idNum <= 99)
                    {
                        if (this.unreserved == null)
                            this.unreserved = new Dictionary<string, string>();
                        this.unreserved[id] = value;
                    }
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseRootContent(nextValue);
            return true;
        }

        private bool parse(string content)
        {
            if (content.Length < 4) return invalid();
            // verify CRC
            var crcValid = QRPay.VerifyCRC(content);
            if (!crcValid) return this.invalid();
            // parse content
            return this.ParseRootContent(content);
        }
        private static string GenFieldData(string id = null, string value = null)
        {
            string fieldId = id ?? "";
            string fieldValue = value ?? "";
            int idLen = fieldId.Length;
            if (idLen != 2 || fieldValue.Length <= 0)
            {
                return "";
            }
            string length = fieldValue.Length.ToString().PadLeft(2, '0');
            return $"{fieldId}{length}{fieldValue}";
        }

        public static QRPay InitVietQR(string bankBin, string bankNumber, string amount = null, string purpose = null, string service = null)
        {
            var qr = new QRPay();
            qr.initMethod = amount != null ? "12" : "11";
            qr.provider.fieldId = FieldID.VIETQR;
            qr.provider.guid = QRProviderGUID.VIETQR;
            qr.provider.name = QRProvider.VIETQR;
            qr.provider.service = service ?? VietQRService.BY_ACCOUNT_NUMBER;
            qr.consumer.bankBin = bankBin;
            qr.consumer.bankNumber = bankNumber;
            qr.amount = amount;
            qr.additionalData.purpose = purpose;
            return qr;
        }

        public static QRPay InitVNPayQR(string merchantId, string merchantName, string store, string terminal, string amount = null, string purpose = null, string billNumber = null, string mobileNumber = null, string loyaltyNumber = null, string reference = null, string customerLabel = null)
        {
            var qr = new QRPay();
            qr.merchant.id = merchantId;
            qr.merchant.name = merchantName;
            qr.provider.fieldId = FieldID.VNPAYQR;
            qr.provider.guid = QRProviderGUID.VNPAY;
            qr.provider.name = QRProvider.VNPAY;
            qr.amount = amount;
            qr.additionalData.purpose = purpose;
            qr.additionalData.billNumber = billNumber;
            qr.additionalData.mobileNumber = mobileNumber;
            qr.additionalData.store = store;
            qr.additionalData.terminal = terminal;
            qr.additionalData.loyaltyNumber = loyaltyNumber;
            qr.additionalData.reference = reference;
            qr.additionalData.customerLabel = customerLabel;
            return qr;
        }

        public string Build()
        {
            string version = QRPay.GenFieldData(FieldID.VERSION, this.version ?? "01");
            string initMethod = QRPay.GenFieldData(FieldID.INIT_METHOD, this.initMethod ?? "11");

            string guid = QRPay.GenFieldData(ProviderFieldID.GUID, this.provider.guid);

            string providerDataContent = "";
            if (this.provider.guid == QRProviderGUID.VIETQR)
            {
                string bankBin = QRPay.GenFieldData(VietQRConsumerFieldID.BANK_BIN, this.consumer.bankBin);
                string bankNumber = QRPay.GenFieldData(VietQRConsumerFieldID.BANK_NUMBER, this.consumer.bankNumber);
                providerDataContent = bankBin + bankNumber;
            }
            else if (this.provider.guid == QRProviderGUID.VNPAY)
            {
                providerDataContent = this.merchant.id ?? "";
            }
            string provider = QRPay.GenFieldData(ProviderFieldID.DATA, providerDataContent);
            string service = QRPay.GenFieldData(ProviderFieldID.SERVICE, this.provider.service);
            string providerData = QRPay.GenFieldData(this.provider.fieldId, guid + provider + service);

            string category = QRPay.GenFieldData(FieldID.CATEGORY, this.category);
            string currency = QRPay.GenFieldData(FieldID.CURRENCY, this.currency ?? "704");
            string amountStr = QRPay.GenFieldData(FieldID.AMOUNT, this.amount);
            string tipAndFeeType = QRPay.GenFieldData(FieldID.TIP_AND_FEE_TYPE, this.tipAndFeeType);
            string tipAndFeeAmount = QRPay.GenFieldData(FieldID.TIP_AND_FEE_AMOUNT, this.tipAndFeeAmount);
            string tipAndFeePercent = QRPay.GenFieldData(FieldID.TIP_AND_FEE_PERCENT, this.tipAndFeePercent);
            string nation = QRPay.GenFieldData(FieldID.NATION, this.nation ?? "VN");
            string merchantName = QRPay.GenFieldData(FieldID.MERCHANT_NAME, this.merchant.name);
            string city = QRPay.GenFieldData(FieldID.CITY, this.city);
            string zipCode = QRPay.GenFieldData(FieldID.ZIP_CODE, this.zipCode);

            string buildNumber = QRPay.GenFieldData(AdditionalDataID.BILL_NUMBER, this.additionalData.billNumber);
            string mobileNumber = QRPay.GenFieldData(AdditionalDataID.MOBILE_NUMBER, this.additionalData.mobileNumber);
            string storeLabel = QRPay.GenFieldData(AdditionalDataID.STORE_LABEL, this.additionalData.store);
            string loyaltyNumber = QRPay.GenFieldData(AdditionalDataID.LOYALTY_NUMBER, this.additionalData.loyaltyNumber);
            string reference = QRPay.GenFieldData(AdditionalDataID.REFERENCE_LABEL, this.additionalData.reference);
            string customerLabel = QRPay.GenFieldData(AdditionalDataID.CUSTOMER_LABEL, this.additionalData.customerLabel);
            string terminal = QRPay.GenFieldData(AdditionalDataID.TERMINAL_LABEL, this.additionalData.terminal);
            string purpose = QRPay.GenFieldData(AdditionalDataID.PURPOSE_OF_TRANSACTION, this.additionalData.purpose);
            string dataRequest = QRPay.GenFieldData(AdditionalDataID.ADDITIONAL_CONSUMER_DATA_REQUEST, this.additionalData.dataRequest);

            string additionalDataContent = buildNumber + mobileNumber + storeLabel + loyaltyNumber + reference + customerLabel + terminal + purpose + dataRequest;
            string additionalData = QRPay.GenFieldData(FieldID.ADDITIONAL_DATA, additionalDataContent);

            // For EVMCo
            string EVMCoContent = string.Join("",
                (this.EVMCo ?? new Dictionary<string, string>()).Keys
                .OrderBy(key => key)
                .Select(key => {
                    this.EVMCo.TryGetValue(key, out string value);
                    return QRPay.GenFieldData(key, value);
                }));

            // For unreserved
            string unreservedContent = string.Join("",
                (this.unreserved ?? new Dictionary<string, string>()).Keys
                .OrderBy(key => key)
                .Select(key => {
                    this.unreserved.TryGetValue(key, out string value);
                    return QRPay.GenFieldData(key, value);
                }));


            string content = $"{version}{initMethod}{providerData}{category}{currency}{amountStr}{tipAndFeeType}{tipAndFeeAmount}{tipAndFeePercent}{nation}{merchantName}{city}{zipCode}{additionalData}{EVMCoContent}{unreservedContent}{FieldID.CRC}04";
            string crc = QRPay.GenCRCCode(content);
            return content + crc;
        }

        public void SetUnreservedField(string id, string value)
        {
            if (this.unreserved == null) this.unreserved = new Dictionary<string, string>();
            this.unreserved[id] = value;
        }
    }
}
