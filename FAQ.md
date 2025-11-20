# Frequently Asked Questions

This document provides an answer for the most common questions gathered from our customers.

## Solution Integration

### Quanto costa la soluzione?
Lato Azure, la soluzione prevede i seguenti costi:
- AAD B2C si paga per MAU e fino a 50000 MAU è completamente free. 1 MAU = 1 Account UNIVOCO che si logga almeno una volta in un mese. Tommaso si logga 500 mila volte con il suo account SPID Aruba? Conta come 1 MAU. Ogni MAU oltre i 50000 costa circa 0,003€ (3 millesimi di Euro)
- App Service per lo SPIDProxy: per HA si consigliano almeno due istanze, il sizing è ovviamente relativo al volume di utenti che ci si aspetta. Consigliamo ALMENO S1. Supponendo 2 istanze S1 = 61 * 2 = 122 €/mese. Lo SPIDProxy può essere installato anche su altri servizi PaaS Azure quali Azure Container Instances, AKS, etc. Su App Service, la best practice è avere AvZone quindi minimo 3 istanze P0v3 che consentirebbero anche l'uso di una reservation per risparmio economico.
- Storage Account: costo trascurabile di pochissimi € al mese.
- Application Insights: il costo è proporzionale all'uso dello SPIDProxy in quanto più richieste = più telemetria. Si parla di qualche €/mese

### Quali sono i prerequisiti per installare la soluzione?
- Se nella vostra organizzazione è stato già effettuato l'accreditamento SPID con AGID, è necessario fornire **l'utimo** metadata inviato ad AGID
- Se l'organizzazione è già accreditata e si vuole utilizzare lo stesso certificato di signing, sarà necessario fornire tale certificato (sia chiave privata che chiave pubblica)
  -  **Non** è obbligatorio utilizzare lo stesso certificato di signing

#### Azure AD B2C
- Accesso global admin al tenant B2C (prod e test) nel caso esista già
- Permessi come contributor su un Resource Group al fine di poter creare le risorse necessarie

#### ADFS
- Accesso al nodo primario ADFS come Administrator

#### SPIDProxy on-prem
- Nel caso in cui non si voglia installare lo SPIDProxy su Azure, sarà necessario avere accesso ad un WebServer Linux/Windows. Lo SPIDProxy dovrà essere raggiungibile tramite internet in https, quindi sarà necessario avere certificato https rilasciato da CA pubblicamente attendibile.

### Avete una demo funzionante della soluzione?
Si, l'ambiente demo utilizza un IdP SPID di Demo messo a disposizione da AGID stessa. Non è un vero IdP SPID ma ne mima completamente il comportamento e può quindi essere utilizzato sia a fini di demo che a fini di verifica del funzionamento della soluzione prima di inviare la richiesta di accreditamento ad AGID.

## Utilizzo della Soluzione

### Come faccio ad integrare la soluzione SPID su un mio applicativo?
#### Azure B2C
AAD B2C è in IdP che supporta diversi protocolli standard: OpenID Connect, OAuth2.0 e SAML. Sarà quindi sufficiente configurare le vostre applicazioni per utilizzare uno di questi protocolli e puntare ad AAD B2C. Si può seguire la documentazione ufficiale per integrare le proprio applicazioni con AAD B2C:
- [Registrare l'applicativo sul tenant B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-register-applications?tabs=app-reg-ga)
- [Code Samples per vari linguaggi/frameworks](https://docs.microsoft.com/en-us/azure/active-directory-b2c/integrate-with-app-code-samples)
- [Applicazioni SAML](https://docs.microsoft.com/en-us/azure/active-directory-b2c/saml-service-provider?tabs=windows&pivots=b2c-custom-policy)
- 
### Come faccio a modificare i valori SPIDL e SPIDACS in base all'applicazione?
Per far sì che una particolare applicazioni usi un valore SPIDL o SPIDACS diverso da quello impostato come default nella configurazione del Proxy, ci sono diversi modi in base al protocollo utilizzato dall'applicazione e in base all'infrastruttura scelta (ADFS o AAD B2C):
- specificare il valore SPIDL o SPIDACS nel parametro wctx (protocollo WS-FED e quindi SOLO con ADFS)
- spceficiare il valore SPIDL o SPIDACS nel RelayState (protocollo SAML, sia su ADFS che AAD B2C. Per AAD B2C sarà necessario utilizzare anche Azure Front Door con custom domain)
- speficiare il valore SPIDL o SPIDACS direttamente in queryString (protocollo OpenIDConnect/OAuth, sia su ADFS che AAD B2C. Per AAD B2C sarà necessario utilizzare anche Azure Front Door con custom domain)
- specificare SPIDL o SPIDACS direttamente nella SAMLRequest, rispettando le regole AGID (protocollo SAML, sia su ADFS che B2C)

**NOTA:** Tutti i meccanismi citati sopra, tranne quello relativo alla SAMLRequest, fanno uso dell'header http "Referer". Tutti i recenti browser hanno una policy di default restrittiva sul passaggio di questo header. Con ADFS, è necessario impostare una Referrer-Policy diversa da quella di default per consentire il passaggio del Referer da ADFS allo SPIDProxy (si fa tramite cmdlet Set-ADFSResponseHeaders: [Customize Response Headers in AD FS](https://docs.microsoft.com/en-us/windows-server/identity/ad-fs/operations/customize-http-security-headers-ad-fs)). Nel caso di AAD B2C, sarà necessario utilizzare Azure Front Door e un custom domain per ottenere lo stesso risultato. La Referrer-Policy va impostata a "no-referrer-when-downgrade". Dettagli sui potenziali valori di Referrer-Policy: [Referrer-Policy reference](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy)

### E' possiibile nell'ambito della vostra soluzione decidere, applicazione per applicazione, se consentire il logon con una qualunqe combinazione tra SPID, CIE e AAD?
La soluzione supporta out-of-the-box CIE, eIDAS e SPID. E' ovviamente possibile customizzarla ulteriormente per aggiungere altri meccanismi di autenticazione come il vostro AAD. In ogni caso, è possibile decidere quali meccanismi di autenticazione sono disponibili per ogni applicazione. Ci sono due vie:
- Gestire un parametro nel querystring di login (chiamiamolo idps) in base al quale far passare dall’applicazione a B2C quali Identity Provider abilitare separati da virgole – e.g. se io volessi avere solo aad e spid mettere “idps=spid,aad")
- Creare diversi user journey (il vero e proprio processo di autenticazione) che verrebbero poi esposti tramite diverse Custom Policy. Le varie applicazioni, quindi, punterebbero tutte a B2C come login, ma con diverse Custom Policy – e.g. gli url di login potrebbero essere `<b2c-url>/SIGNIN_SPID`, `<b2c-url>/SIGNIN_AAD`, `<b2c-url>/SIGNIN_SPID_AAD`, etc.

Questa seconda opzione dà un controllo centrale maggiore, perché se un domani voleste aggiungere il login tramite eIDAS a tutte le applicazioni che si possono loggare con SPID, basterebbe aggiungere eIDAS nelle custom policy SIGNIN_SPID e SIGNIN_SPID&AAD senza toccare le applicazioni, mentre nel primo caso ogni applicazione dovrebbe aggiungere il valore eIDAS tra i valori del parametro idps nel querystring. Diciamo che, considerando di avere un set di modalità di login predefinite limitato (come nel vostro caso), risulta più vantaggiosa a livello gestionale.
Per approfondimenti su questa soluzione puoi fare riferimento a [questa docs](https://docs.microsoft.com/en-us/azure/active-directory-b2c/userjourneys#claims-provider-selection).

### E' possibile utilizzare la vostra soluzione in ambito IDEM/eduGAIN in presenza di IdP Shibboleth? Se si, a quali condizioni?
Si può fare. L’automatizzazione della soluzione (per evitare di doversi scrivere a mano ogni IDP appartenente a eduGAIN) richiederebbe la realizzazione di uno script ad hoc che vada a fare il parsing del file xml metadata di eduGAIN e che vada poi a scrivere in append queste informazioni al file custom policy di B2C.

### Avete forse a disposizione una sorta di guida, per integrare con la vostra soluzione degli SP di tipo simplesamlphp o Shibboleth?
Gli SP che usano la libreria simplesamlphp o shibboleth non sono altro che applicazioni che usano protocollo SAML. Quindi si, si possono [integrare con AAD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/saml-service-provider?tabs=windows&pivots=b2c-custom-policy). Si tratta solo di configurare le librerie per "puntare" ad AAD B2C.
N.B.: Di default le custom policies che crei in b2c supportano solo protocollo Oauth2/OpenIDConnect. Per supportare SAML vanno create delle policy ad-hoc. Quindi andranno fatte delle modifiche alle policy per consentire lo scenario.

### Come faccio a richiedere dati particolari ad AGID?
Nel metadata mandato ad AGID sono presenti diversi livelli di AttributeConsumingService (ACS), identificati da un index. Nel Proxy è presente un valore di default che specifica quale set di ACS utilizzare, tra quelli indicati ad AGID. E' possibile sovrascrivere il valore di default utilizzando uno dei metodi indicati nella sezione *"Come faccio a modificare i valori SPIDL e SPIDACS in base all'applicazione?"*

### Come faccio a richiedere l'autorizzazione su diversi livelli di sicurezza (username e password, MFA, etc)?
Questo è possibile sfruttando lo SPID Level (SPIDL). Nel proxy è presente un valore di default che specifica quale SPIDL verrà utilizzato dagli applicativi. E' possibile sovrascrivere il valore di default utilizzando uno dei metodi indicati nella sezione *"Come faccio a modificare i valori SPIDL e SPIDACS in base all'applicazione?"*

### Avviene un qualche tipo di matching tra utenti?
Di default, no. Ogni account SPID è un'utenza B2C. Quindi se la stessa persona ha un account SPID per ogni provider (Poste, TIM, Aruba, etc) allora esisteranno N utenti distinti in B2C. È possibile fare un matching tra utenti, anche se questa logica non è inclusa nelle Custom Policies di default, per fare in modo che account SPID provenienti da diversi IdP ma appartenenti alla stessa persona vengano mappati sullo stesso utente tramite il Codice Fiscale. Lo stesso tipo di matching può essere utilizzato su ADFS per abbinare un account SPID ad un'utenza Active Directory.

L'unico attributo che identifica in maniera univoca una persona su SPID è il Codice Fiscale. Avendo il Codice Fiscale si può effettuare il seguente flow:
1. Quanto un utente si autentica tramite federated account (SPID, AAD, CIE, etc.), viene ritornato anche il Codice Fiscale
2. Prima di creare un utente federato in B2C, si cerca un'utenza con quel CF
3. Se trovo il CF, non creo un altro utente, ma aggiungo quell'identità federata all'utente pre-esistente
4. Se non trovo il CF, creo un utente federato

### Posso loggarmi su Office365 tramite SPID?
Sì, ma solo se la configurazione attuale del tenant AAD prevede un dominio "federated" con ADFS.
*Esempio*
Dal momento in cui si effettua login su SharePoint (o qualsiasi altra applicazione della suite Office365), si viene rimandati al login del vostro tenant AAD. Se AAD è federato con ADFS, si finisce quindi su ADFS, che a valle delle modifiche apportate con la nostra soluzione, mosta tra le opzioni di login anche SPID/CIE/eIDAS/. A questo punto si può effettuare il login utilizzando SPID. Una volta eseguito il login con SPID, ADFS cercherà su Active Directory un'utenza con quel particolare codice fiscale e ritornerà ad Office365 le info necessarie per completare l'accesso. **NOTA**: e' necessario avere un attributo con il codice fiscale su tutte le utenze Active Directory.

## Manutenzione della Soluzione

### Nel caso in cui SPID venga creato un nuovo IdP, la cosa viene recepita automaticamente oppure bisogna intervenire manualmente?
La cosa non verrebbe recepita in automatico. Andrebbe aggiunto il nuovo IdP nella configurazione ADFS/B2C e dello SPID Proxy. Andrebbe poi aggiunto il nuovo logo dell'IdP sull'interfaccia grafica. In sostanza, si tratterebbe di aggiungere il nuovo IdP tramite configurazione ove necesario.

### Ammesso che venga rilasciata una versione aggiornata di SPID, quante ore sono richieste per effettuare l'update?
Dipende da quale componente della soluzione viene aggiornato. Nel peggiore dei casi una giornata, in generale 4 ore.

### In caso di bug, in quanto tempo prevediamo di risolverli?
Non offriamo SLA sulla tempesitivtà di risoluzione bug, ma non appena ci arriva una segnalazione e confermiamo si tratta di un bug "nostro", interveniamo il più tempestivamente possibile. Il conseguente update viene comunque offerto/erogato tramite contratto Premier quindi vale "il costo" in ore come da domanda precedente.

### Nel caso in cui AGID introduca delle modifiche tecniche che sia nostro interesse o necessità recepire, possiamo contare su un vostro intervento tempestivo di adeguamento?
Assolutamente si. L'attività viene sempre erogata tramite contratto Premier, quindi avere un contratto sottoscritto è un pre-requisito.
