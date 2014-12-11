﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Cbis.ProductManagement.Client.Generated;

namespace Cbis.ProductManagement.Client
{
    /// <summary>
    /// A management client to communicate with the Cbis system to manage suppliers and product.
    /// </summary>
    public class CbisSupplierManagementClient
    {
        private readonly InformationSystemManagementClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CbisSupplierManagementClient" /> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        /// <param name="userName">Name of the user that you would like to connect as.</param>
        /// <param name="password">The password valid for the <paramref name="userName" />.</param>
        /// <exception cref="System.ArgumentNullException">
        /// connectionString
        /// or
        /// userName
        /// or
        /// password
        /// </exception>
        public CbisSupplierManagementClient(string endpointConfigurationName, string remoteAddress, string userName, string password)
        {
            if(string.IsNullOrEmpty(endpointConfigurationName))
            {
                throw new ArgumentNullException("endpointConfigurationName");
            }

            if (string.IsNullOrEmpty(remoteAddress))
            {
                throw new ArgumentNullException("remoteAddress");
            }

            _client = new InformationSystemManagementClient(endpointConfigurationName, remoteAddress);
            SetCrendentials(userName, password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CbisSupplierManagementClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.ArgumentNullException">endpointConfigurationName as null</exception>
        public CbisSupplierManagementClient(string endpointConfigurationName, string userName, string password)
        {
            if (string.IsNullOrEmpty(endpointConfigurationName))
            {
                throw new ArgumentNullException("endpointConfigurationName");
            }

            _client = new InformationSystemManagementClient(endpointConfigurationName);
            SetCrendentials(userName, password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CbisSupplierManagementClient" /> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.ArgumentNullException">
        /// binding
        /// or
        /// endpoint
        /// </exception>
        public CbisSupplierManagementClient(Binding binding, EndpointAddress endpoint, string userName, string password)
        {
            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            _client = new InformationSystemManagementClient(binding, endpoint);
            SetCrendentials(userName, password);
        }

        /// <summary>
        /// Creates a supplier in the Cbis system with the specified information.
        /// </summary>
        /// <param name="name">The name of the organization.</param>
        /// <param name="referenceName">A name reference that the organization is known by in your system.</param>
        /// <param name="uiCulture">The UI culture.</param>
        /// <param name="languageCulture">The language culture.</param>
        /// <returns>An id which the organization is know by in the Cbis</returns>
        /// <exception cref="System.ArgumentNullException">
        /// name
        /// or
        /// referenceName
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// uiCulture;May not be of Invariant type
        /// or
        /// languageCulture;May not be of Invariant type
        /// </exception>
        public ReferenceName CreateSupplier(string name, ReferenceName referenceName, CultureInfo uiCulture, CultureInfo languageCulture)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (referenceName == null)
            {
                throw new ArgumentNullException("referenceName");
            }

            if (CultureInfo.InvariantCulture.Equals(uiCulture))
            {
                throw new ArgumentOutOfRangeException("uiCulture", "May not be of Invariant type");
            }

            if (CultureInfo.InvariantCulture.Equals(languageCulture))
            {
                throw new ArgumentOutOfRangeException("languageCulture", "May not be of Invariant type");
            }

            var orgReference = _client.CreateOrganization(name, new OrganizationReference(referenceName) , uiCulture.Name, languageCulture.Name);
            
            return new ReferenceName(orgReference.SubSystem, orgReference.LocalName);
        }

        public void UpdateSupplier(ReferenceName organizationReference, List<InformationData> informationData)
        {
            if (organizationReference == null)
            {
                throw new ArgumentNullException("organizationReference");
            }

            if (informationData == null)
            {
                throw new ArgumentNullException("informationData");
            }

            var orgRef = new OrganizationReference(organizationReference);

            var unifiedItems = this.InformationDataValidator(informationData);
            var dataList = unifiedItems.Select(x => x.Create()).ToArray();

            _client.UpdateOrganization(orgRef, dataList);
        }

        public ReferenceName SetProduct(ReferenceName organizationReference, string name, int categoryId, ReferenceName productId, 
            IEnumerable<InformationData> informationData, IEnumerable<ImageData> images, IEnumerable<Occasion> occasions)
        {
            if (organizationReference == null)
            {
                throw new ArgumentNullException("organizationReference");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (productId == null)
            {
                throw new ArgumentNullException("productId");
            }

            HashSet<InformationData> unifiedItems = InformationDataValidator(informationData);

            OrganizationReference orgRef = new OrganizationReference(organizationReference);
            Generated.ReferenceName prodRef = new Generated.ReferenceName(productId);

            if (occasions == null)
            {
                occasions = new List<Occasion>();
            }
            
            if (images == null)
            {
                images = new List<ImageData>();
            }

            ProductCallbackReference productResult = _client.SetProduct(orgRef, name, categoryId, prodRef,
                unifiedItems.Select(x => x.Create()).ToArray(), 
                images.Select(x => x.Create()).ToArray(), 
                occasions.Select(x => x.Create()).ToArray());

            return new ReferenceName(productResult.CbisProductId.SubSystem, productResult.CbisProductId.LocalName);
        }

        public HashSet<InformationData> InformationDataValidator(IEnumerable<InformationData> data)
        {
            HashSet<InformationData> validator = new HashSet<InformationData>(new InfodataComparer());
            foreach (var item in data)
            {
                if (!validator.Add(item))
                {
                    // Only kick if they are not identical
                    var conflictItem = validator.First(x => x.AttributeId == item.AttributeId && x.Language.Equals(item.Language));
                    if (!conflictItem.Equals(item))
                    {
                        throw new InvalidOperationException("Duplicate item of type: " + item.AttributeId + " with language: " + item.Language);
                    }
                }
            }

            return validator;
        }

        public List<Category> GetCategories()
        {
            var categories = _client.GetSystemCategories();
            var list = new List<Category>();

            foreach (var ct in categories)
            {
                list.Add(new Category(ct.Id, ct.Name));
            }

            return list;
        }

        public Product GetProduct(ReferenceName organizationReference, ReferenceName productReference)
        {
            var orgRef = new OrganizationReference(organizationReference);
            var prodRef = new Generated.ReferenceName(productReference);
            var converter = this.GetInformationDataConverter();

            var product = _client.GetProduct(orgRef, prodRef);

            var productReferenceName = new ReferenceName(product.Reference.SubSystem, product.Reference.LocalName);
            var informationDataList = new List<InformationData>();

            foreach (var data in product.InformationData)
            {
                Func<Generated.InformationData, InformationData> dataConverter;

                if (!converter.TryGetValue(data.GetType(), out dataConverter))
                {
                    throw new Exception(/*TODO*/);
                }

                informationDataList.Add(dataConverter(data));
            }

            var occasions = new List<Occasion>();

            foreach (var occasion in product.OccasionData)
            {
                occasions.Add(ConvertOccasion(occasion));
            }

            var images = new List<ImageData>();

            foreach (var media in product.MediaData)
            {
                images.Add(ConvertMedia(media));
            }

            return new Product(productReferenceName, informationDataList, occasions, images);
        }

        public List<ProductReference> GetProducts(ReferenceName organizationReference, int pageSize, int skipPages)
        {
            if (organizationReference == null)
            {
                throw new ArgumentNullException("organizationReference");
            }

            OrganizationReference orgRef = new OrganizationReference(organizationReference);

            var productList = _client.GetProducts(orgRef, pageSize, skipPages);

            var list = new List<ProductReference>();

            foreach (var p in productList)
            {
                list.Add(new ProductReference(p.SystemName, p.ReferenceNames));
            }

            return list;
        }

        /// <summary>
        /// Gets all system cultures.
        /// </summary>
        /// <returns>List of CultureInfos</returns>
        public List<CultureInfo> GetCultures()
        {
            var cultureInfoArray = this._client.GetSystemCultures();

            return cultureInfoArray.ToList();
        }

        private void SetCrendentials(string userName, string password)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("_client needs to be initialized before call");
            }

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            //_client.ClientCredentials.UserName.UserName = userName;
            //_client.ClientCredentials.UserName.Password = password;

            _client.ChannelFactory.Credentials.UserName.UserName = userName;
            _client.ChannelFactory.Credentials.UserName.Password = password;
        }

        private class InfodataComparer : IEqualityComparer<InformationData>
        {
            public bool Equals(InformationData x, InformationData y)
            {
                return x.AttributeId == y.AttributeId && x.Language.Equals(y.Language);
            }

            public int GetHashCode(InformationData obj)
            {
                return obj.AttributeId.GetHashCode() ^ obj.Language.GetHashCode();
            }
        }

        private Dictionary<Type, Func<Generated.InformationData, InformationData>> GetInformationDataConverter()
        {
            var converter = new Dictionary<Type, Func<Generated.InformationData, InformationData>>();

            converter.Add(typeof(Generated.InformationDataBoolean), this.ConvertInformationDataBoolean);
            converter.Add(typeof(Generated.InformationDataDouble), this.ConvertInformationDataDouble);
            converter.Add(typeof(Generated.InformationDataInt), this.ConvertInformationDataInteger);
            converter.Add(typeof(Generated.InformationDataString), this.ConvertInformationDataString);

            return converter;
        }

        private InformationData ConvertInformationDataBoolean(Generated.InformationData infoData)
        {
            var infoDataBool = infoData as Generated.InformationDataBoolean;

            if (infoDataBool == null)
            {
                throw new ArgumentException("Must be InformationDataBoolean", "infoData");
            }

            var cultureInfo = this.ConvertCulture(infoData.Culture);

            var convertedInfoDataBool = new InformationDataBool(cultureInfo, infoDataBool.AttributeId, infoDataBool.Value);

            return convertedInfoDataBool;
        }

        private InformationData ConvertInformationDataDouble(Generated.InformationData infoData)
        {
            var infoDataDouble = infoData as Generated.InformationDataDouble;

            if (infoDataDouble == null)
            {
                throw new ArgumentException("Must be InformationDataDouble", "infoData");
            }

            var cultureInfo = this.ConvertCulture(infoData.Culture);

            var convertedInfoDataDouble = new InformationDataDouble(cultureInfo, infoDataDouble.AttributeId, infoDataDouble.Value);

            return convertedInfoDataDouble;
        }

        private InformationData ConvertInformationDataInteger(Generated.InformationData infoData)
        {
            var infoDataInteger = infoData as Generated.InformationDataInt;

            if (infoDataInteger == null)
            {
                throw new ArgumentException("Must be InformationDataInt", "infoData");
            }

            var cultureInfo = this.ConvertCulture(infoData.Culture);

            var convertedInfoDataInteger = new InformationDataInt(cultureInfo, infoDataInteger.AttributeId, infoDataInteger.Value);

            return convertedInfoDataInteger;
        }

        private InformationData ConvertInformationDataString(Generated.InformationData infoData)
        {
            var infoDataString = infoData as Generated.InformationDataString;

            if (infoDataString == null)
            {
                throw new ArgumentException("Must be InformationDataString", "infoData");
            }

            var cultureInfo = this.ConvertCulture(infoData.Culture);

            var convertedInfoDataString = new InformationDataString(cultureInfo, infoDataString.AttributeId, infoDataString.Value);

            return convertedInfoDataString;
        }

        private Occasion ConvertOccasion(Generated.OccasionInformationData occasion)
        {
            if (!occasion.StartDate.HasValue || !occasion.EndDate.HasValue)
            {
                throw new ArgumentOutOfRangeException("occasion", occasion, "occasion must have a start value and an end value.");
            }

            if (occasion.StartTime.HasValue && occasion.EndTime.HasValue)
            {
                return new Occasion(occasion.StartDate.Value, occasion.EndDate.Value, (DayOfWeekMask)occasion.ValidDays, occasion.StartTime.Value, occasion.EndTime.Value);
            }

            return new Occasion(occasion.StartDate.Value, occasion.EndDate.Value, (DayOfWeekMask)occasion.ValidDays);
        }

        private ImageData ConvertMedia(Generated.MediaInformationData media)
        {
            return new ImageData(media.Location);
        }

        private CultureInfo ConvertCulture(string culture)
        {
            CultureInfo cultureInfo;

            if (string.IsNullOrEmpty(culture))
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
            else
            {
                cultureInfo = new CultureInfo(culture);
            }

            return cultureInfo;
        }
    }
}