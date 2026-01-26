// AuthenticationService for MSAL Blazor WebAssembly
// This provides the JavaScript interop layer that Blazor's MSAL package expects

(function () {
    let msalInstance = null;
    let msalConfig = null;
    let accountId = null;
    let initPromise = null;
    let isInitialized = false;

    window.AuthenticationService = {
        init: async function (options, logger) {
            console.log('AuthenticationService.init called', options);
            
            // Prevent double initialization
            if (isInitialized) {
                console.log('AuthenticationService already initialized, skipping');
                return;
            }
            
            if (initPromise) {
                console.log('AuthenticationService initialization in progress, waiting...');
                return await initPromise;
            }
            
            initPromise = (async () => {
                try {
                    // Blazor MSAL passes options with 'auth' not 'authentication'
                    const authOptions = options.auth || options.authentication || {};
                    
                    if (!authOptions.clientId) {
                        console.warn('AuthenticationService: No clientId provided, skipping MSAL init');
                        isInitialized = true;
                        return;
                    }
                    
                    msalConfig = {
                        auth: {
                            clientId: authOptions.clientId,
                            authority: authOptions.authority,
                            redirectUri: authOptions.redirectUri || window.location.origin + '/authentication/login-callback',
                            postLogoutRedirectUri: authOptions.postLogoutRedirectUri || window.location.origin,
                            navigateToLoginRequestUrl: true
                        },
                        cache: {
                            cacheLocation: options.cache?.cacheLocation || 'sessionStorage',
                            storeAuthStateInCookie: false
                        },
                        system: {
                            loggerOptions: {
                                logLevel: 3, // Info
                                loggerCallback: (level, message, containsPii) => {
                                    console.log(`MSAL [${level}]: ${message}`);
                                }
                            }
                        }
                    };

                    console.log('Creating MSAL instance with config:', msalConfig);
                    msalInstance = new msal.PublicClientApplication(msalConfig);
                    
                    console.log('Initializing MSAL...');
                    await msalInstance.initialize();
                    console.log('MSAL initialized successfully');

                    // Handle redirect callback
                    try {
                        console.log('Handling redirect promise...');
                        const response = await msalInstance.handleRedirectPromise();
                        if (response) {
                            accountId = response.account?.homeAccountId;
                            console.log('Redirect login successful', response.account?.username);
                        } else {
                            console.log('No redirect response (normal for fresh page load)');
                        }
                    } catch (error) {
                        console.error('Error handling redirect:', error);
                    }

                    // Check for existing accounts
                    const accounts = msalInstance.getAllAccounts();
                    console.log('Found accounts:', accounts.length);
                    if (accounts.length > 0) {
                        accountId = accounts[0].homeAccountId;
                        console.log('Found existing account:', accounts[0].username);
                    }
                    
                    isInitialized = true;
                    console.log('AuthenticationService.init completed successfully');
                } catch (error) {
                    console.error('AuthenticationService.init failed:', error);
                    isInitialized = true; // Mark as initialized to prevent infinite retries
                    throw error;
                }
            })();
            
            return await initPromise;
        },

        getUser: async function () {
            if (!msalInstance) {
                console.log('getUser: MSAL not initialized');
                return null;
            }

            const accounts = msalInstance.getAllAccounts();
            if (accounts.length === 0) {
                console.log('getUser: No accounts found');
                return null;
            }

            const account = accounts[0];
            console.log('getUser: Returning account', account.username);
            console.log('getUser: ID token claims', account.idTokenClaims);

            // Get roles from ID token claims
            const roles = account.idTokenClaims?.roles || [];
            console.log('getUser: Roles', roles);

            // Return user profile in the format Blazor expects
            // Include roles for authorization
            return {
                name: account.name,
                preferred_username: account.username,
                oid: account.localAccountId,
                sub: account.localAccountId,
                tid: account.tenantId,
                roles: roles
            };
        },

        getAccessToken: async function (options) {
            if (!msalInstance) {
                console.log('getAccessToken: MSAL not initialized');
                return { status: 'RequiresRedirect' };
            }

            const accounts = msalInstance.getAllAccounts();
            if (accounts.length === 0) {
                console.log('getAccessToken: No accounts, requires redirect');
                return { status: 'RequiresRedirect' };
            }

            const account = accounts[0];
            const scopes = options?.scopes || [];

            try {
                const response = await msalInstance.acquireTokenSilent({
                    scopes: scopes,
                    account: account
                });

                console.log('getAccessToken: Token acquired silently');
                return {
                    status: 'Success',
                    token: {
                        value: response.accessToken,
                        expires: response.expiresOn,
                        grantedScopes: response.scopes
                    }
                };
            } catch (error) {
                console.log('getAccessToken: Silent token acquisition failed, requires redirect', error);
                return { status: 'RequiresRedirect' };
            }
        },

        signIn: async function (context) {
            console.log('signIn called', context);
            
            if (!msalInstance) {
                return { status: 'Failure', errorMessage: 'MSAL not initialized' };
            }

            try {
                const loginRequest = {
                    scopes: context?.interactiveRequest?.scopes || ['openid', 'profile', 'email'],
                    state: context?.state ? JSON.stringify(context.state) : undefined
                };

                await msalInstance.loginRedirect(loginRequest);
                return { status: 'Redirect' };
            } catch (error) {
                console.error('signIn error:', error);
                return { status: 'Failure', errorMessage: error.message };
            }
        },

        completeSignIn: async function (url) {
            console.log('completeSignIn called', url);
            
            if (!msalInstance) {
                return { status: 'OperationCompleted' };
            }

            try {
                const response = await msalInstance.handleRedirectPromise();
                if (response) {
                    accountId = response.account?.homeAccountId;
                    console.log('completeSignIn: Login successful', response.account?.username);
                    return { 
                        status: 'Success', 
                        state: response.state ? JSON.parse(response.state) : null 
                    };
                }
                return { status: 'OperationCompleted' };
            } catch (error) {
                console.error('completeSignIn error:', error);
                return { status: 'Failure', errorMessage: error.message };
            }
        },

        signOut: async function (context) {
            console.log('signOut called', context);
            
            if (!msalInstance) {
                return { status: 'Failure', errorMessage: 'MSAL not initialized' };
            }

            try {
                const accounts = msalInstance.getAllAccounts();
                if (accounts.length > 0) {
                    const account = accounts[0];
                    
                    // Try popup logout first (works better with navigation)
                    try {
                        await msalInstance.logoutPopup({
                            account: account,
                            postLogoutRedirectUri: msalConfig.auth.postLogoutRedirectUri,
                            mainWindowRedirectUri: msalConfig.auth.postLogoutRedirectUri
                        });
                    } catch (popupError) {
                        console.log('Popup logout failed, clearing cache manually', popupError);
                        // If popup fails, clear cache manually
                        msalInstance.clearCache();
                        sessionStorage.clear();
                        localStorage.removeItem('msal.idtoken');
                        localStorage.removeItem('msal.accesstoken');
                    }
                }
                
                accountId = null;
                
                // Redirect to home
                window.location.href = msalConfig.auth.postLogoutRedirectUri || '/';
                return { status: 'Success' };
            } catch (error) {
                console.error('signOut error:', error);
                // Even on error, try to clear and redirect
                sessionStorage.clear();
                window.location.href = '/';
                return { status: 'Success' };
            }
        },

        completeSignOut: async function (url) {
            console.log('completeSignOut called', url);
            accountId = null;
            return { status: 'Success' };
        }
    };
})();
