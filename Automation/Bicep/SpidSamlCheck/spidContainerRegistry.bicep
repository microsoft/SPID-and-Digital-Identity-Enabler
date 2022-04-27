param sku object 
param name string
param environmentType string

resource spidProxyContainerRegistry 'Microsoft.ContainerRegistry/registries@2021-06-01-preview'={
  name: name
  location: resourceGroup().location
  sku: sku
  properties:{
    adminUserEnabled:true
  }
  tags:{
    Name:'INL-SPID'
    Company:'INL'
    Role:'Container Registry'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment:environmentType
  }
}
output ContainerRegistryName string = spidProxyContainerRegistry.name
