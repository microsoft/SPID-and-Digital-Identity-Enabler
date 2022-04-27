param allowedOrigin string 
param sku object
param kind string
param name string
param environmentType string

resource spidCustomUIStorageAccount 'Microsoft.Storage/storageAccounts@2021-04-01'={
  name: name
  location: resourceGroup().location
  sku: sku
  kind: kind
  properties:{
    
  }
  tags:{
    Name:'INL-SPID'
    Company:'INL'
    Role:'Storage Account'
    BizOwner:'TBD'
    AppOwner:'TBD'
    UpTime:'Full time'
    Environment:environmentType
  }

  resource corssettings 'blobServices@2021-04-01' ={
    name: 'default'
    properties:{
      cors:{
        corsRules:[
          {
            allowedHeaders: [
              '*'
            ]
            allowedOrigins:[
              allowedOrigin
            ]
            maxAgeInSeconds: 200
            exposedHeaders: [
              '*'
            ]
            allowedMethods:[
              'GET'
              'OPTIONS'
            ]
          }
        ]
      }
    }
    dependsOn: [
      spidCustomUIStorageAccount
    ]

    resource customuicontainer 'containers@2021-04-01'={
      name: 'customui'
      properties:{
        publicAccess:'Blob'
      }
      dependsOn: [
        spidCustomUIStorageAccount
      ]  
    }
    resource metadata 'containers@2021-04-01'={
      name: 'metadatas'
      properties:{
        publicAccess:'Blob'
      }
      dependsOn: [
        spidCustomUIStorageAccount
      ]  
    }

     /* resource logs 'containers@2021-04-01'={
      name: logFolder
      properties:{
        publicAccess:'None'
      }
    }  */
  }
}

/* //create container
resource customuicontainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-02-01' = {
  name: '${spidCustomUIStorageAccount.name}/default/customui'
  dependsOn: [
    spidCustomUIStorageAccount
  ]
} */
/* resource metadata 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-02-01' = {
  name: '${spidCustomUIStorageAccount.name}/default/metatada'
  dependsOn: [
    spidCustomUIStorageAccount
  ]
}

resource corssettings 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01'={
  name: '${spidCustomUIStorageAccount.name}/default'
  properties:{
    cors:{
      corsRules:[
        {
          allowedHeaders: [
            '*'
          ]
          allowedOrigins:[
            allowedOrigin
          ]
          maxAgeInSeconds: 200
          exposedHeaders: [
            '*'
          ]
          allowedMethods:[
            'GET'
            'OPTIONS'
          ]
        }
      ]
    }
  }
  dependsOn:[
    spidCustomUIStorageAccount
  ]
} */

output spidCustomUIStorageAccountId string = spidCustomUIStorageAccount.id
