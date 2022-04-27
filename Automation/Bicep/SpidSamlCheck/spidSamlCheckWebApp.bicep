param spidSamlCheckAppPlanID string
param spidSamlCheckAppInsigthsInstrumentationKey string
param spidSamlCheckAppInsightsConnectionString string
param name string
param environmentType string

resource spidSamlCheckWebApp 'Microsoft.Web/sites@2021-01-15' = {
  name: name
  location: resourceGroup().location
  properties:{
    reserved:true
    serverFarmId:spidSamlCheckAppPlanID

    siteConfig:{
      appSettings:[
        {
          name:'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: spidSamlCheckAppInsigthsInstrumentationKey
        }
        {
          name:'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: spidSamlCheckAppInsightsConnectionString
        }          
      ]
      alwaysOn:true 
    }
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
