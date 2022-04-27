param spidProxyAppPlanID string
param spidProxyAppInsigthsInstrumentationKey string
param spidProxyAppInsightsConnectionString string
param ADFSAttributeConsumerServiceUrl string
param SPIDL string
param UpdateAssertionConsumerServiceUrl string
param WEBSITE_LOAD_CERTIFICATES string
param ADFSEntityId string
param OriginalEntityId string
param SPIDSAMLCHECK string
param SPIDSAMLCHECK_LOGOUT string
param CERTNAME string
param CERTPASSWORD string
param SpidSamlCheckMetadata string
param environmentType string


param name string

resource spidProxyWebApp 'Microsoft.Web/sites@2018-02-01'={
  name: name
  location: resourceGroup().location
  properties:{
    siteConfig:{
      appSettings:[
        {
          name:'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: spidProxyAppInsigthsInstrumentationKey
        }
        {
          name:'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: spidProxyAppInsightsConnectionString
        }
        {
          name:'ADFSAttributeConsumerServiceUrl'
          value:ADFSAttributeConsumerServiceUrl  
        }
        {
          name:'SPIDL'
          value:SPIDL  
        }
        {
          name:'UpdateAssertionConsumerServiceUrl'
          value:UpdateAssertionConsumerServiceUrl  
        }
        {
          name:'WEBSITE_LOAD_CERTIFICATES'
          value:WEBSITE_LOAD_CERTIFICATES  
        }
        {
          name:'ADFSEntityId'
          value:ADFSEntityId  
        }
        {
          name:'OriginalEntityId'
          value:OriginalEntityId  
        }
        {
          name:'SPIDSAMLCHECK'
          value:SPIDSAMLCHECK  
        }
        {
          name:'SPIDSAMLCHECK_LOGOUT'
          value:SPIDSAMLCHECK_LOGOUT  
        }
        {
          name:'CERTNAME'
          value:CERTNAME  
        }
        {
          name:'CERTPASSWORD'
          value:CERTPASSWORD  
        }
        {
          name:'https://spidsamlcheck.azurewebsites.net'
          value:SpidSamlCheckMetadata
        }
      ]
      
      netFrameworkVersion:'v4.0'
      alwaysOn:true
      phpVersion: 'OFF'
    }
    serverFarmId: spidProxyAppPlanID
  }
  tags:{
    Name:'INL-SPID'
    Company:'INL'
    Role:'Web App'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment:environmentType
  }
}

