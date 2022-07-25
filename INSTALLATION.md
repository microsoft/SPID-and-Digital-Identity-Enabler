# SPID Proxy Installation Steps
Questa guida vuole essere un supporto per gli utenti che hanno necessità di configurare e installare la soluzione SPID proxy.

## Prerequisites

- An Azure subscription. If you don't have one, create a [free account](https://azure.microsoft.com/free/?WT.mc_id=A261C142F) before you begin.

- An Azure account that's been assigned at least the [Contributor](../role-based-access-control/built-in-roles.md) role within the subscription or a resource group within the subscription is required. 

- Installare all'interno di visual studio code l'estensione per Azure AD B2C [B2C-vscode] (https://github.com/azure-ad-b2c/vscode-extension)
  

## Creazione delle risorse  
1. Creazione di un tenant Azure AD B2C
2. Creazione un App Service per l'installazione dello SPID Proxy
3. Creazione di uno storage account su cui andranno caricati i file della UI custom e i metadati necessari per la configurazione di Azure AD B2C

## Configurazioni
1. Configurazione di Identity experience framework
Per poter utilizzare le custom policy all'interno di Azure AD B2C è necessario creare e configurare le Key, come indicato nel seguente indirizzo [CreateUserFlowAndCustomPolicy](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#add-signing-and-encryption-keys-for-identity-experience-framework-applications). 
E' disponibile anche il seguente tool per aiutare l'utente nella configurazione iniziale di Azure AD B2C [b2ciefsetupapp] (https://b2ciefsetupapp.azurewebsites.net/). Andando a indicare il nnome del tenant, il tool crea automaticamente le applicazioni e carica automaticamente le policy dello starte pack all'interno del tenant.

2. Modificare gli id e le informazioni all'interno dell'appsettings delle custom policies. Le informazioni da inserire    
   1. Modificare il Name e il Tenant
   2. Modificare IdentityExperienceFrameworkAppId
   3. Modificare ProxyIdentityExperienceFrameworkAppId
   4. Modificare MetadatasBlobStorageUrl. Per convenzione {url-static-website}/metadata 
   5. Modificare CustomUiBlobStorageUrl Per convenzione {url-static-website}/customui
   6. Modificare AppInsightsKey
   7. Modificare CNSSAMLMetadata solo nel caso sia necessario applicare la configurazione CNS
   8. Creare un self signed certificate seguendo la seguente guida a questo link [certificate] (https://docs.microsoft.com/en-us/azure/active-directory-b2c/identity-provider-adfs-saml?tabs=windows&pivots=b2c-custom-policy#create-a-self-signed-certificate). Una volta generato dovrà essere caricato su Azure AD B2C. In fase di creazione bisognerà specificare il nome del tenant e la durata del certificato. Successivamente esportare il certificato, ricordandosi di selezionare yes nella checkbox per l'esportazione della provate key e specificando una password. L'encription da selezionare è TripleDeS-SHA1. Dopo questa operazione bisognerà esportare il certificato in una folder. Successivamente andare sul tenant Azure AD B2C e caricare una policy key chiamata SigninKey e caricando il certificato specificando la password che abbiamo scelto precedentemente.

3. Buildare le policy dentro visual studio code e poi caricarle all'interno del tenant
4. Caricare la UI customizzata. Per eseguire questa operazione sarà necessario caricare i file nelle folder apposite e modificare all'interno dei files i placeholder con i valori indicati nell'apssettings.json. Successivamente sarà necessario configurare anche i CORS sullo storage indicando il dominio del tenant per i verbi GET e OPTION
5. Configurare e caricare i metadati per puntamento allo SPIDProxy.Generare il certificato per lo SPIDProxy usando il tool specifico di AGID. Le regole sono indicate all'interno del seguente link [AGID] (https://www.agid.gov.it/sites/default/files/repository_files/spid-avviso-n29v3-specifiche_sp_pubblici_e_privati_0.pdf)
6. Configurare SPIDProxy specificando alcuni settings all'interno dell'app service. I settings più importanti sono i seguent:
   1. Sezione Certificate: specificare il nome del pfx e la sua password
   2. Sezione Federator: specificare "SPIDEntityId","EntityId","FederatorAttributeConsumerServiceUrl"




