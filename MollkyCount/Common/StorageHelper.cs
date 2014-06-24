using System;
using System.Net;
using System.Windows;
using System.Runtime.Serialization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;


namespace MollkyCount.Common
{
    public class StorageHelper
    {

        public static async Task<IStorageFile> GetFile(string name)
        {
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = null;
            try
            {
                file = await folder.GetFileAsync(name);
            }
            catch(Exception)
            {
                
            }

            if (file == null)
            {
                file = await folder.CreateFileAsync(name);
            }

            return file;
        }

        public static async Task<T> Load<T>(string name) where T : class, new()
        {
            T loadedObject = null;
            string str = string.Empty;

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                var file = await folder.GetFileAsync(name);

                using (StreamReader reader = new StreamReader(
                     await folder.OpenStreamForReadAsync(name)))
                {
                    str = await reader.ReadToEndAsync();

                }
               
                using (Stream fileStream = await file.OpenStreamForReadAsync())
                {
                    loadedObject = serializer.ReadObject(fileStream) as T;
                }
            }
            catch (Exception)
            {
                loadedObject = new T();
            }

            //var otherString = str.ToLower();
            return loadedObject;
        }

        public static async Task Save<T>(string name, T objectToSave)
        {

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            StorageFile file = null;
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                file = await folder.GetFileAsync(name);
                await file.DeleteAsync();
            }
            catch (Exception)
            {
            }

            file = await folder.CreateFileAsync(name);

            if (file != null)
            {
                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    serializer.WriteObject(fileStream, objectToSave);
                    fileStream.Flush();
                }
            }
        }

        //public static void Delete(string name)
        //{
        //    using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        storageFile.Remove();
        //    }
        //}
    }

}
