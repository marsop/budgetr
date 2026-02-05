// Google Drive API JavaScript interop for Budgetr
window.googleDriveInterop = {
    // Configuration
    _clientId: null,
    _tokenClient: null,
    _accessToken: null,
    _userEmail: null,
    _isInitialized: false,
    
    // Backup file name in Google Drive
    BACKUP_FILENAME: 'budgetr-backup.json',
    
    // Initialize the Google API client
    initialize: async function(clientId) {
        if (this._isInitialized && this._clientId === clientId) {
            return true;
        }
        
        this._clientId = clientId;
        
        try {
            // Wait for Google Identity Services to load
            await this._waitForGis();
            
            // Initialize token client
            this._tokenClient = google.accounts.oauth2.initTokenClient({
                client_id: clientId,
                scope: 'https://www.googleapis.com/auth/drive.file https://www.googleapis.com/auth/userinfo.email',
                callback: (response) => {
                    if (response.access_token) {
                        this._accessToken = response.access_token;
                    }
                }
            });
            
            // Check for existing token in local storage
            const savedToken = localStorage.getItem('budgetr_gdrive_token');
            if (savedToken) {
                this._accessToken = savedToken;
                const email = await this._fetchUserEmail();
                if (!email) {
                    // Token expired or invalid
                    this._accessToken = null;
                    localStorage.removeItem('budgetr_gdrive_token');
                }
            }
            
            this._isInitialized = true;
            return true;
        } catch (error) {
            console.error('Failed to initialize Google Drive:', error);
            return false;
        }
    },
    
    // Wait for Google Identity Services library to load
    _waitForGis: function() {
        return new Promise((resolve, reject) => {
            if (typeof google !== 'undefined' && google.accounts && google.accounts.oauth2) {
                resolve();
                return;
            }
            
            let attempts = 0;
            const maxAttempts = 50;
            const interval = setInterval(() => {
                attempts++;
                if (typeof google !== 'undefined' && google.accounts && google.accounts.oauth2) {
                    clearInterval(interval);
                    resolve();
                } else if (attempts >= maxAttempts) {
                    clearInterval(interval);
                    reject(new Error('Google Identity Services library failed to load'));
                }
            }, 100);
        });
    },
    
    // Fetch user email from Google
    _fetchUserEmail: async function() {
        if (!this._accessToken) return null;
        
        try {
            const response = await fetch('https://www.googleapis.com/oauth2/v2/userinfo', {
                headers: { 'Authorization': `Bearer ${this._accessToken}` }
            });
            
            if (response.ok) {
                const data = await response.json();
                this._userEmail = data.email;
                return data.email;
            }
        } catch (error) {
            console.error('Failed to fetch user email:', error);
        }
        return null;
    },
    
    // Check if user is signed in
    isSignedIn: function() {
        return !!this._accessToken;
    },
    
    // Get the signed-in user's email
    getUserEmail: function() {
        return this._userEmail;
    },
    
    // Sign in with Google
    signIn: function() {
        return new Promise((resolve, reject) => {
            if (!this._tokenClient) {
                reject(new Error('Google Drive not initialized'));
                return;
            }
            
            // Override callback for this specific sign-in
            this._tokenClient.callback = async (response) => {
                if (response.error) {
                    reject(new Error(response.error));
                    return;
                }
                
                if (response.access_token) {
                    this._accessToken = response.access_token;
                    localStorage.setItem('budgetr_gdrive_token', response.access_token);
                    await this._fetchUserEmail();
                    resolve(true);
                } else {
                    reject(new Error('No access token received'));
                }
            };
            
            // Request access token
            // Removed prompt: 'consent' to avoid forcing the consent screen every time
            this._tokenClient.requestAccessToken();
        });
    },
    
    // Sign out
    signOut: async function() {
        if (this._accessToken) {
            try {
                google.accounts.oauth2.revoke(this._accessToken);
            } catch (error) {
                console.warn('Error revoking token:', error);
            }
        }
        
        this._accessToken = null;
        this._userEmail = null;
        localStorage.removeItem('budgetr_gdrive_token');
    },
    
    // Find the backup file in Google Drive
    _findBackupFile: async function() {
        if (!this._accessToken) {
            throw new Error('Not signed in');
        }
        
        const query = `name='${this.BACKUP_FILENAME}' and trashed=false`;
        const url = `https://www.googleapis.com/drive/v3/files?q=${encodeURIComponent(query)}&fields=files(id,name,modifiedTime)`;
        
        const response = await fetch(url, {
            headers: { 'Authorization': `Bearer ${this._accessToken}` }
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error?.message || 'Failed to search files');
        }
        
        const data = await response.json();
        return data.files && data.files.length > 0 ? data.files[0] : null;
    },
    
    // Get the content of the latest backup
    getLatestBackup: async function() {
        const file = await this._findBackupFile();
        if (!file) {
            return null;
        }
        
        const url = `https://www.googleapis.com/drive/v3/files/${file.id}?alt=media`;
        const response = await fetch(url, {
            headers: { 'Authorization': `Bearer ${this._accessToken}` }
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error?.message || 'Failed to download file');
        }
        
        return await response.text();
    },
    
    // Save backup to Google Drive
    saveBackup: async function(content) {
        if (!this._accessToken) {
            throw new Error('Not signed in');
        }
        
        const existingFile = await this._findBackupFile();
        
        if (existingFile) {
            // Update existing file
            const url = `https://www.googleapis.com/upload/drive/v3/files/${existingFile.id}?uploadType=media`;
            const response = await fetch(url, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${this._accessToken}`,
                    'Content-Type': 'application/json'
                },
                body: content
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error?.message || 'Failed to update file');
            }
            
            return await response.json();
        } else {
            // Create new file
            const metadata = {
                name: this.BACKUP_FILENAME,
                mimeType: 'application/json'
            };
            
            const form = new FormData();
            form.append('metadata', new Blob([JSON.stringify(metadata)], { type: 'application/json' }));
            form.append('file', new Blob([content], { type: 'application/json' }));
            
            const response = await fetch('https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this._accessToken}`
                },
                body: form
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error?.message || 'Failed to create file');
            }
            
            return await response.json();
        }
    },
    
    // Get last modified time of backup file
    getBackupLastModified: async function() {
        const file = await this._findBackupFile();
        if (!file || !file.modifiedTime) {
            return null;
        }
        return file.modifiedTime;
    }
};
