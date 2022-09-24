# SPID Proxy Installation Steps
Questa guida vuole essere un supporto per tutti gli utenti che hanno necessità di configurare e installare la soluzione SPID proxy funzionante.

## Prerequisites
- Una sottoscrizione Azure. Se non è presente crearne una [free account](https://azure.microsoft.com/free/?WT.mc_id=A261C142F) prima di iniziare.


## Addons
- Installare all'interno di visual studio code l'estensione [B2Cvscode](https://github.com/azure-ad-b2c/vscode-extension) utile per lavorare con Azure AD B2C.
  

## Creazione delle risorse
Come prima cosa è necessario creare le risorse utili per poter pubblicare SPID Proxy. Le azioni da fare sono le seguenti:  
- Creare un tenant Azure AD B2C all'interno del portale seguendo la seguente esercitazione: [Esercitazione: Creare un tenant di Azure Active Directory B2C](https://learn.microsoft.com/it-it/azure/active-directory-b2c/tutorial-create-tenant)
- Creare un App Service per l'installazione dello SPID Proxy seguendo la seguente quickstart: [Quickstart: Deploy an ASP.NET web app](https://learn.microsoft.com/en-in/azure/app-service/quickstart-dotnetcore?tabs=net60&pivots=development-environment-azure-portal)
- Creare uno storage account, che servirà per avere un posto dove caricare i file della UI Custom e i metadati necessari per la configurazione di Azure AD B2C. Per dubbi segui questo articolo: [Create Storage Account](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) 

## Configurazioni
### Configurazione Identity experience framework
Le custom policy sono file di configurazione che definiscono il comportamento del tenant di Azure Active Directory B2C (Azure AD B2C). Le custom policy possono essere completamente modificate per configurare e personalizzare molte attività di diverso tipo. 

Per poter utilizzare le custom policy è necessario creare le signing and encryption keys all'interno di Azure AD B2C come indicato nel seguente articolo: [Creazione signing and encryption keys](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#add-signing-and-encryption-keys-for-identity-experience-framework-applications). 
Nel caso si voglia automatizzare il processo di configurazione di Identity Experience Framework è disponibile il seguente tool [B2C Identity Experience Framework setup](https://b2ciefsetupapp.azurewebsites.net/). 


### Modifica degli id e delle informazioni all'interno dell'appsettings delle custom policies. 
Le informazioni da inserire sono:   
   1. Modificare il Name e il Tenant
   2. Modificare IdentityExperienceFrameworkAppId
   3. Modificare ProxyIdentityExperienceFrameworkAppId
   4. Modificare MetadatasBlobStorageUrl. Per convenzione {url-static-website}/metadata 
   5. Modificare CustomUiBlobStorageUrl Per convenzione {url-static-website}/customui
   6. Modificare AppInsightsKey
   7. Modificare CNSSAMLMetadata solo nel caso sia necessario applicare la configurazione CNS
   8. Creare un self signed certificate seguendo la seguente guida a questo link [certificate] (https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-adfs-saml?tabs=windows&pivots=b2c-custom-policy#create-a-self-signed-certificate). Una volta generato dovrà essere caricato su Azure AD B2C. In fase di creazione bisognerà specificare il nome del tenant e la durata del certificato. Successivamente esportare il certificato, ricordandosi di selezionare yes nella checkbox per l'esportazione della provate key e specificando una password. L'encription da selezionare è TripleDeS-SHA1. Dopo questa operazione bisognerà esportare il certificato in una folder. Successivamente andare sul tenant Azure AD B2C e caricare una policy key chiamata SigninKey e caricando il certificato specificando la password che abbiamo scelto precedentemente.

3. Build e upload delle policy all'interno del tenant
   Seguendo le normale procedure di upload si caricano le policy all'interno del tenant
   
4. Upload della UI customizzata
   Per eseguire questa operazione sarà necessario caricare i file nelle folder apposite e modificare all'interno dei files i placeholder con i valori indicati nell'apssettings.json. Successivamente sarà necessario configurare anche i CORS sullo storage indicando il dominio del tenant per i verbi GET e OPTION
5. Configurazione e upload deimetadati per puntamento allo SPIDProxy.
   Generare il certificato per lo SPIDProxy usando il tool specifico di AGID. Le regole sono indicate all'interno del seguente link [AGID] (https://www.agid.gov.it/sites/default/files/repository_files/spid-avviso-n29v3-specifiche_sp_pubblici_e_privati_0.pdf)
6. Configurazione dello SPIDProxy
    Specificare alcuni settings all'interno dell'app service. I settings più importanti sono i seguent:
   1. Sezione Certificate: specificare il nome del pfx e la sua password
   2. Sezione Federator: specificare "SPIDEntityId","EntityId","FederatorAttributeConsumerServiceUrl"




