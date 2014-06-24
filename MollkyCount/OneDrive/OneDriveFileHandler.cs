using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Live;
using System.IO;
using System.Windows;
using MollkyCount.Common;
using MollkyCount.DAL;
using System.Threading;

namespace MollkyCount.OneDrive
{
    /// <summary>
    /// SkyDrive file manager
    /// </summary>
    public class OneDriveFileHandler
    {
        #region Data Members
        private string folderID; //folder's id in skydrive
        private string clientID; //skydrive application client id
        private readonly string folderName;


        //LiveConnect members
        private LiveConnectClient liveClient;
        private LiveAuthClient liveAuth;
        private LiveLoginResult liveResult;
        private LiveConnectSession liveSession;
        private string[] requiredScopes;
        #endregion

        #region Ctor's
        /// <summary>
        /// Initializes SkyDriveFileHandler
        /// </summary>
        /// <param name="folder">The folder in SkyDrive which contains app's files</param>
        /// <param name="clientID">Application ID in SkyDrive</param>
        /// <param name="scopes">Required SkyDrive scopes</param>
        public OneDriveFileHandler(LiveConnectSession session, string clientID, string folder, string[] scopes = null)
        {
            this.clientID = clientID;
            liveSession = session;
            folderName = folder;

            if (scopes == null)
            {
                //setting scopes by default
                requiredScopes = new string[] { "wl.basic", "wl.skydrive", "wl.offline_access", "wl.signin", "wl.skydrive_update" };
            }
        }
        #endregion

        #region Private Methods

        public async Task Initialize()
        { 
            await GetSkyDriveFolder(); 
        }

        private async Task SignIn()
        {
            if (liveSession == null)
            {
                liveAuth = new LiveAuthClient(clientID);
                liveResult = await liveAuth.InitializeAsync(requiredScopes);

                if (liveResult.Status != LiveConnectSessionStatus.Connected)
                {
                    liveResult = await liveAuth.LoginAsync(requiredScopes);
                }

                liveSession = liveResult.Session;
                liveClient = new LiveConnectClient(liveSession);

            }
        }

        /// <summary>
        /// Gets the id for a existing skydrive folder or creates a new one and gets an id for it
        /// </summary>
        /// <returns></returns>
        private async Task GetSkyDriveFolder()
        {
            if (liveClient == null)
                await SignIn();

            //the session is already established, so let's find our folder by its name
            LiveOperationResult result = await liveClient.GetAsync("me/skydrive/files/");
            List<object> data = (List<object>)result.Result["data"];
            foreach (IDictionary<string, object> content in data)
            {
                if (content["name"].ToString() == folderName)
                {
                    //The folder has been found!
                    folderID = content["id"].ToString();
                }
            }
            //the folder hasn't been found, so let's create a new one
            if (folderID == null)
                folderID = await CreateSkydriveFolder();

        }

        /// <summary>
        /// Creates a new skydrive folder and returns its id
        /// </summary>
        /// <returns></returns>
        private async Task<string> CreateSkydriveFolder()
        {
            string id = null;
            try
            {
                var folderData = new Dictionary<string, object>();
                folderData.Add("name", folderName);
                LiveOperationResult operationResult =
                    await liveClient.PostAsync("me/skydrive", folderData);
                dynamic result = operationResult.Result;
                id = string.Format("{0}", result.id);
            }
            catch 
            {
                //...something is wrong...
            }
            return id;
        }

        /// <summary>
        /// moves downloaded file into app's local storage
        /// </summary>
        /// <param name="fileName">Name of a downloaded file</param>
        /// <param name="fileID">File's ID in SkyDrive</param>
        /// <returns></returns>
        private async Task MoveToLocalStorage(string fileName, string fileID)
        {
            var file = await StorageHelper.GetFile(fileName);

            try
            {
                var res = await liveClient.BackgroundDownloadAsync(fileID + "/content", file, new CancellationToken(false), null);
            }
            catch (Exception ex)
            {
            }

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Upload a file from local storage to a SkyDrive's folder
        /// </summary>
        /// <param name="fileName">Name of a file in LocalStorage</param>
        /// <returns></returns>
        public async Task<OperationStatus> UploadFile(string fileName)
        {
            var file = await StorageHelper.GetFile(fileName);

            if (file != null)
            {
                try
                {
                    if (liveClient == null)
                        await GetSkyDriveFolder();

                    LiveOperationResult res = await liveClient.BackgroundUploadAsync(folderID,
                                                fileName,
                                                file,
                                                OverwriteOption.Overwrite
                                                );

                    return OperationStatus.Completed;
                }
                catch
                {
                    return OperationStatus.Failed;
                }
            }

            return OperationStatus.Failed;
        }

        /// <summary>
        /// Downloads a file from SkyDrive and saves it in LocalStorage
        /// If the file exists it will be overwritten
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<OperationStatus> DownloadFile(string fileName)
        {
            string fileID = null;

            if (liveClient == null)
                await GetSkyDriveFolder();

            //looking for the file
            LiveOperationResult fileResult = await liveClient.GetAsync(folderID + "/files");
            List<object> fileData = (List<object>)fileResult.Result["data"];
            foreach (IDictionary<string, object> content in fileData)
            {
                if (content["name"].ToString() == fileName)
                {
                    //The file has been found!
                    fileID = content["id"].ToString();
                    await MoveToLocalStorage(fileName, fileID);
                    return OperationStatus.Completed;
                }
            }

            return OperationStatus.Failed;
        } //DownloadFile

        /// <summary>
        /// Compares files in the local storage and SkyDrive folder
        /// Gets the newest file and updates the older one
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //public async Task<OperationStatus> SynchronizeFile(string fileName)
        //{
        //    string fileID = null;

        //    //looking for the file in skydrive folder
        //    LiveOperationResult fileResult = await liveClient.GetAsync(folderID + "/files");
        //    List<object> fileData = (List<object>)fileResult.Result["data"];
        //    foreach (IDictionary<string, object> content in fileData)
        //    {
        //         if (content["name"].ToString() == fileName)
        //         {
        //             //The file has been found!
        //             fileID = content["id"].ToString();
        //             DateTime fileUpdatedTime = new DateTime();
        //             if (DateTime.TryParse(content["updated_time"].ToString(), out fileUpdatedTime))
        //             {
        //                 using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
        //                 {
        //                     if (isoStore.FileExists(fileName))
        //                     {
        //                         //the file already exists in the local storage. let's compare dates
        //                         if (fileUpdatedTime > isoStore.GetLastWriteTime(fileName).DateTime)
        //                         {
        //                             await MoveToLocalStorage(fileName, fileID);
        //                         }
        //                         else if (fileUpdatedTime < isoStore.GetLastWriteTime(fileName).DateTime)
        //                         {
        //                             //local file is newer than skydrive file
        //                             //updating skydrive file
        //                             await UploadFile(fileName);
        //                         }                                 
        //                     }
        //                     else
        //                     {
        //                         //the file hasn't been found in LocalStorage
        //                         //so we're downloading it and move into app's localstorage
        //                         await MoveToLocalStorage(fileName, fileID);
        //                     }
        //                     return OperationStatus.Completed;
        //                  }
        //              }
        //        }                
        //    }
        //    //upload the file to skydrive if file hasn't been found there 
        //    if (fileID == null)
        //    {
        //        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
        //        {
        //            if (isoStore.FileExists(fileName))
        //            {
        //                await UploadFile(fileName);
        //                return OperationStatus.Completed;
        //            }
        //        }                
        //    }

        //    return OperationStatus.Failed;

        //} //SynchronizeFile

        #endregion       


    }

    public enum OperationStatus
    {
        Completed, Failed
    }
}
