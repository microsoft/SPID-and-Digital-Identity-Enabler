# SPID Proxy Installation Steps
Questa guida vuole essere un supporto per tutti gli utenti che hanno necessità di configurare e installare la soluzione SPID proxy funzionante.

## Prerequisites
- Una sottoscrizione Azure. Se non è presente crearne una [free account](https://azure.microsoft.com/free/?WT.mc_id=A261C142F) prima di iniziare.


## Addons
- Installare all'interno di visual studio code l'estensione [B2Cvscode](https://github.com/azure-ad-b2c/vscode-extension) utile per lavorare con Azure AD B2C.
  

## Creazione delle risorse
Come prima cosa è necessario creare le risorse utili per poter pubblicare SPID Proxy. Le azioni da fare sono le seguenti:  
- Creare un tenant Azure AD B2C all'interno del portale seguendo la seguente esercitazione: [Esercitazione: Creare un tenant di Azure Active Directory B2C](https://learn.microsoft.com/it-it/azure/active-directory-b2c/tutorial-create-tenant)
- Creare un App Service per la pubblicazione dello SPID Proxy, seguendo la seguente quickstart: [Quickstart: Deploy an ASP.NET web app](https://learn.microsoft.com/en-in/azure/app-service/quickstart-dotnetcore?tabs=net60&pivots=development-environment-azure-portal)
- Creare uno storage account, che servirà per avere un posto dove caricare i file della UI Custom e i metadati necessari per la configurazione di Azure AD B2C. Per informazioni segui questo articolo: [Create Storage Account](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) 
- Creare una risorsa Application Insight all'interno della propria sottoscrizione, non all'intenro del tenant Azure B2C. Non appena abbiamo creato la risorsa ci occorrera' l'Instrumentation Key, utile per permetterci di inviare gli event data verso Application Insight. Per informazioni fare riferimento a questo articolo [Track user behavior in Azure AD B2C by using Application Insights](https://learn.microsoft.com/en-us/azure/active-directory-b2c/analytics-with-application-insights?pivots=b2c-custom-policy#create-an-application-insights-resource)

## Configurazioni
### Configurazione Identity experience framework
Le custom policy sono file di configurazione che definiscono il comportamento del tenant di Azure Active Directory B2C (Azure AD B2C). Le custom policy possono essere completamente modificate per configurare e personalizzare molte attività di diverso tipo. 

Per poter utilizzare le custom policy è necessario creare le chiavi signing e encryption keys all'interno di Azure AD B2C come indicato nel seguente articolo: [Creazione signing and encryption keys](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#add-signing-and-encryption-keys-for-identity-experience-framework-applications).
Successivamente bisognera' registrare le applicazioni Identity Experience Framework, che sono:
- IdentityExperienceFramework 
- ProxyIdentityExperienceFramework 

Per informazioni sulla creazione di queste applicazioni fare riferimento alla [documentazione](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#register-identity-experience-framework-applications)

Una comoda alternativa è il seguente tool [B2C Identity Experience Framework setup](https://b2ciefsetupapp.azurewebsites.net/) che permette di automatizzare il processo di configurazione di Identity Experience Framework.

Quando il tenant è pronto e abbiamo tutte le chiavi necessarie per la configurazione dell'identity experience framework, bisogna modificare il file [appsetting.json](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/appsettings.json) andando a specificare all'interno della sezione PolicySettings le chiavi e i valori necessari:
```json
 "PolicySettings": {
            "IdentityExperienceFrameworkAppId": "",
            "ProxyIdentityExperienceFrameworkAppId": "",
            "MetadatasBlobStorageUrl": "The url to the container/folder which contains the SPID IdP edited metadatas",
            "CustomUiBlobStorageUrl": "The url to the contianer/folder which contains the customUI",
            "AppInsightsKey": "The AppInsights key used by custom policies",
            "CNSSAMLMetadata": "The metadata url for the CNS middleware. Used only if you want to use CNS"
        }
```

**IdentityExperienceFrameworkAppId e ProxyIdentityExperienceFrameworkAppId**
sono i client id delle application registrate per la configurazione dell'identity experience framework.

**MetadatasBlobStorageUrl e CustomUiBlobStorageUrl** 
sono gli indirizzi dove sono collocati i file dei metadati e della custom ui. Nell'esempio specifico è stato utilizzato uno storage, ma è possibile usare anche altri servizi. L'importante è che i files siano raggiungibili tramite HTTPS e che i CORS siano correttamente configurati.

**AppInsightsKey** è la chiave per agganciare la telemetria di application insight.

**CNSSAMLMetadata** si utilizza solo nel caso in cui si vuole abilitare il meccanismo di autenticazione tramite CNS. Nello specifico bisognerà inserire l'url statico dell'applicazione web contente i metadata necessari.

### Creazione delle signin Key ###
All'interno dello starter pack di Azure B2C non sono presenti identity provider SAML, quindi bisogna definirne uno ad hoc che, dopo averlo caricato su Azure B2C, dovrà essere definito all'interno della policy [TrustFrameworkExtension](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/TrustFrameworkExtensions.xml#L141). Il certificato verrà utilizzato da Azure B2C per firmare tutte le richieste verso SpidProxy.

Come prima cosa bisognerà creare un certificato seguendo la seguente guida: [creare un certificato self-signed](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-adfs-saml?tabs=windows&pivots=b2c-custom-policy#create-a-self-signed-certificate). 
```powershell
New-SelfSignedCertificate `
    -KeyExportPolicy Exportable `
    -Subject "CN=yourtenant.onmicrosoft.com" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature `
    -NotAfter (Get-Date).AddYears(10) `
    -CertStoreLocation "Cert:\CurrentUser\My"
```
Dopo aver creato il cerficato bisognerà esportarlo in locale ricordandosi di selezionare export privete key. Successivamente bisognerà selezionare e inserire una password e assicurandosi che sia selezionata come Encryption TripleDeS-SHA1, in quanto l'altra su Azure B2C non funziona.

Successivamente bisognerà accedere al tenant B2C e caricare il certificato generando una chiave che verrà poi inserita all'interno della [TrustFrameworkExtension](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/TrustFrameworkExtensions.xml#L141) policy.

Arrivati a questo punto sarà necessario caricare le custom policy su tenant Azure B2C.

### Caricamento della UI Custom Storage Account ###

**L'utilizzo dello storage account è solo una possibilità, infatti i file della UI e le custom policy possono essere posizionati in qualsiasi posto, purchè sia raggiungibile tramite HTTPS e i CORS siano configurati correttamente.**

Accedere allo Storage account e configurare il CORS, cliccando su Resource Sharing e impostando nel blob service l'origine https://.b2clogin.com e gli allowed methods (GET e OPTIONS). Dopo avere inserito star (*) in Allowed Header e in Exposed Header, impostare un Max Age di 200.

Successivamente salvare la configurazione CORS e procedere con il caricamento dei file statici.

Nella folder della CustomUI sono presenti tutti i file della UI che dovranno essere caricati nello storage. All'interno di questi file sono presenti dei placeholder che fanno riferimento alle proprietà CustomUiBlobStorageUrl e MetadatasBlobStorageUrl del file [appsetting.json](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/appsettings.json).

Trovando i placeholder dentro i files della customUI e sostituendo il valore con l'url rispettivo i file sono pronti per essere caricati nello storage nella cartella customUI dentro $web. 
**Per poter visualizzare il container $web bisogna abilitare la feature static website dentro lo storage account.**

### Caricamento dei Metadata dentro lo Storage Account ###
Nei file dei metadati dobbiamo sostituire gli endpoint con quelli dello spidproxy e dobbiamo generare un certificato dello spidproxy seguendo le norme AGID. 

Per creare il certifcato dello spidproxy dobbiamo utilizzare il tool [spid compliant certificate](https://github.com/italia/spid-compliant-certificates). Le indicazioni per creare il certificato sono indicate a questo link [Avviso SPID n.29 v3](https://www.agid.gov.it/sites/default/files/repository_files/spid-avviso-n29v3-specifiche_sp_pubblici_e_privati_0.pdf). 
Da normativa e' possibile creare un certificato per i soggetti pubblici, mentre per i privati si crea un CSR e lo si spedisce ad AGID che a sua volta rispndera' inviando a sua volta un certificato valido.

Copiare il file public.env.example e rinominarlo in docker.env. Succesivamente modificarne i valori all'interno indicando i valori del soggetto pubblico. Le informazioni sono ricercabili sul sito di AGID. 
Consigliamo di fare riferimento al repository [spid compliant certificate](https://github.com/italia/spid-compliant-certificates) per impostare correttamente tutti i valori di configurazione.


Dopo avere settato questi valori seguire la [guida](https://github.com/italia/spid-compliant-certificates#private-key-csr-and-self-signed-certificate-for-public-sector-with-docker) eseguendo lo script gencert-with-docker.sh.
Dopo che le chiavi sono state generate correttamente, bisognerà unirle tramite openssl. Per farlo bisognerà usare il certificato pubblico (crt.pem) e la chiave (key.pem) tramite il seguente comando
```bash
openssl pkcs12 -export -out outputfile.pfx -inkey key.pem -in crt.pem
```
Il pfx sarà da caricare all'interno dello spidproxy, mentre il crt.pem servirà per generare i metadata modificati degli IDP di SPID. E' presente uno [script powershell](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/Powershell%20Scripts/Get-SPIDMetadatas.ps1) che aiuta in questo processo.
Si dovrà copiare il crt.pem in formato stringa all'interno della folder con lo script powershell successivamente si eseguirà lo script, indicando anche l'url dello spid proxy all'interno del file
```powershell
Get-SPIDMetadatas.ps1 -SPIDProxyBaseUrl "url-spid-proxy" -additionalSPIDProxyBaseUrl "SPIDProxy parallelo per persone giuridiche (opzionale)"  -certificateFilePath "path del certificato"
```
Lo script copierà i metadata originali modificandoli con i parametri che sono stati inseriti nella linea di comando. I file saranno cosi pronti da caricare sullo storage account. La folder dove inserli è "metadatas".

## Pubblicazione dello SPID Proxy
L'ultimo passo da seguire è la pubblicazione dello SPID Proxy. Bisognerà scaricare dalle release lo .zip di SpidProxy. Accedere all'app service e tramite Kudu utilizzare la funzione Zip push deploy per caricare lo zip della release. 
Per il caricamento del certificato si possono seguire due procedure:
- Caricare il certificato all'interno di un Azure Key Vault (procedura consigliata). Per dubbi seguire la seguente [guida](https://learn.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate?tabs=azure-portal).
- Caricare il file pfx generato dentro la folder SigninCert posizionata nella root dello spid proxy
Il deploy ora è terminato.

## Configurazione SPIDProxy
Dopo aver ultimato il deploy rimane solamente da configurare l'applicazione nei suoi parametri di configurazione. 
All'interno del portale, nella pagina di configurazione di App Service bisognerà andare ad aggiungere dei parametri di configurazione che sono:
- Certificate__CertName come indicato [qui](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/WebApps/Proxy/Microsoft.SPID.Proxy/appsettings.json#L38)
- Certificate__CertPassword come indicato [qui](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/WebApps/Proxy/Microsoft.SPID.Proxy/appsettings.json#L39)

Successivamente la sezione che va modificata [qui](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/WebApps/Proxy/Microsoft.SPID.Proxy/appsettings.json#L41) nello specifico:
- **Federator__EntityId**: entity id di Azure B2C.
- **Federator__SPIDEntityId**: entity id scritto nel metadata che verrà inviato a AGID.
- **Federator__FederatorAttributeConsumerServiceUrl**: Url dove bisogna mandare le risposte dallo SPIDProxy verso Azure B2C.
Non è necessario modificare altre configurazioni.

La fase successiva riguarda l'upload delle custom policy all'interno di Azure Active Directory B2C. Questo si puo' fare in due modi:
 - manualmente dal portale di Azure 
 - tramite l'estensione per [vs code](https://marketplace.visualstudio.com/items?itemName=AzureADB2CTools.aadb2c).

