param name string
param environmentType string

resource spidProxyWebAppApplicationInsigths 'Microsoft.Insights/components@2020-02-02'={
  name: name
  location: resourceGroup().location
  kind:'web' 
  properties:{
    Application_Type:'web'
    Flow_Type:'Bluefield'
    IngestionMode:'ApplicationInsights'
  }
  tags:{
    Name:'INL-SPID'
    Company:'INL'
    Role:'Application Insigths'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment:environmentType
  }
}

output spidProxyInstrumentationKey string = spidProxyWebAppApplicationInsigths.properties.InstrumentationKey
output spidProxyInstrumentationKeyConnectionString string = spidProxyWebAppApplicationInsigths.properties.ConnectionString
