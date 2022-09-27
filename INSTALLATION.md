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

## Configurazioni
### Configurazione Identity experience framework
Le custom policy sono file di configurazione che definiscono il comportamento del tenant di Azure Active Directory B2C (Azure AD B2C). Le custom policy possono essere completamente modificate per configurare e personalizzare molte attività di diverso tipo. 

Per poter utilizzare le custom policy è necessario creare le signing and encryption keys all'interno di Azure AD B2C come indicato nel seguente articolo: [Creazione signing and encryption keys](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#add-signing-and-encryption-keys-for-identity-experience-framework-applications).

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
sono le chiavi generate per la configurazione dell'identity experience framework.

**MetadatasBlobStorageUrl e CustomUiBlobStorageUrl** 
sono gli indirizzi dove sono collocati i file dei metadati e della cusotm ui. Nell'esempio specifico è stato utilizzato uno storage, ma è possibile usare anche altri posti. L'importante è che i files siano raggiungibili tramite HTTPS e che i CORS siano correttamente configurati.

**AppInsightsKey** è la chiave per agganciare la telemetria di application insight.

**CNSSAMLMetadata** si utilizza solo nel caso in cui si vuole abilitare il meccanismo di autenticazione tramite CNS. Nello specifico bisognerà inserire l'url statico dell'applicazione web contente i metadata necessari.

### Creazione delle signin Key ###
All'interno dello starter pack di Azure B2C non sono presenti identity provider SAML, quindi bisogna definirne uno ad hoc che, dopo averlo caricato su Azure B2C, dovrà essere definito all'interno della policy [TrustFrameworkExtension](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/TrustFrameworkExtensions.xml#L141). Il certificato verrà utilizzato da Azure B2C per firmare tutte le richieste verso SpidProxy.

Come prima cosa bisognerà creare un certificato seguendo la seguente guida: [creare un certificato self-signed](https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-adfs-saml?tabs=windows&pivots=b2c-custom-policy#create-a-self-signed-certificate). 
```powershell
New-SelfSignedCertificate `
    -KeyExportPolicy Exportable `
    -Subject "CN=yourappname.yourtenant.onmicrosoft.com" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature `
    -NotAfter (Get-Date).AddMonths(20) `
    -CertStoreLocation "Cert:\CurrentUser\My"
```
Dopo aver creato il cerficato bisognerà esportarlo in locale ricordandosi di selezionare export privete key. Successivamente bisognerà selezionare e inserire una password e assicurandosi che sia selezionata come Encryption TripleDeS-SHA1, in quanto l'altra su Azure B2C non funziona.

Successivamente bisognerà accedere al tenant B2C e caricare il certificato generando una chiave che verrà poi inserita all'interno della [TrustFrameworkExtension](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/TrustFrameworkExtensions.xml#L141) policy.

Arrivati a questo punto sarà necessario caricare le custom policy su tenant Azure B2C.

### Caricamento della UI Custom Storage Account ###
**L'utilizzo dello storage account non è una prerogrativa, anzi come già indicato precedentemente nella guida, i file della UI e le policy possono essere posizionati in qualsiasi posto, purchè sia raggiungibile in HTTPS e i CORS siano configurati correttamente.**

Accedere allo Storage account e configurare il cors, cliccando su Resource Shareing e impostando im blob service l'origine (l'url del sito statico) e gli allowed methods (GET e OPTIONS). Dopo avere inserito * in Allowed header and in Exposed header, sarà necessario impostare anche un Max age (200). Successivamente salvare la configurazione CORS e procedere con il caricamento dei file statici.

Nella folder della CustomUI sono presenti tutti i file della UI che dovranno essere caricati nello storage. Dentro ognuno di questi file sono presenti dei placeholder che riferiscono alle proprietà CustomUiBlobStorageUrl e MetadatasBlobStorageUrl del file [appsetting.json](https://github.com/microsoft/SPID-and-Digital-Identity-Enabler/blob/main/AAD%20B2C/CustomPolicies/appsettings.json).

Trovando i placeholder dentro i files della customUI e sostituendo il valore con l'url rispettivo i file sono pronti per essere caricati nello storage nella cartella customUI dentro $web.

### Caricamento dei Metadata dentro lo Storage Account ###
Nei metadati dobbiamo modificare gli endpoint per prevedere gli endpoint dello spidproxy e il certificato dello spidproxy. Prima cosa dobbiamo generare il certifcato dello spidproxy utilizzando il tool [spid compliant certificate](https://github.com/italia/spid-compliant-certificates). Le regole per creare il certificato sono indicate a questo link> [Avviso SPID n.29 v3](https://www.agid.gov.it/sites/default/files/repository_files/spid-avviso-n29v3-specifiche_sp_pubblici_e_privati_0.pdf).






