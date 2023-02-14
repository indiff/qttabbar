//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace QTTabBarLib.Common
{
    /// <summary>Defines properties for known folders that identify the path of standard known folders.</summary>
    public static class KnownFolders
    {
        /// <summary>Gets a strongly-typed read-only collection of all the registered known folders.</summary>
        public static ICollection<IKnownFolder> All
        {
            get
            {
                return GetAllFolders();
            }
        }

        /// <summary>Gets the metadata for the <b>CommonOEMLinks</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CommonOemLinks
        {
            get
            {
                return GetKnownFolder(FolderIdentifiers.CommonOEMLinks);
            }
        }

        /// <summary>Gets the metadata for the <b>DeviceMetadataStore</b> folder.</summary>
        public static IKnownFolder DeviceMetadataStore
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.DeviceMetadataStore);
            }
        }

        /// <summary>Gets the metadata for the <b>DocumentsLibrary</b> folder.</summary>
        public static IKnownFolder DocumentsLibrary
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.DocumentsLibrary);
            }
        }

        /// <summary>Gets the metadata for the <b>ImplicitAppShortcuts</b> folder.</summary>
        public static IKnownFolder ImplicitAppShortcuts
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.ImplicitAppShortcuts);
            }
        }

        /// <summary>Gets the metadata for the <b>Libraries</b> folder.</summary>
        public static IKnownFolder Libraries
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.Libraries);
            }
        }

        /// <summary>Gets the metadata for the <b>MusicLibrary</b> folder.</summary>
        public static IKnownFolder MusicLibrary
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.MusicLibrary);
            }
        }

        /// <summary>Gets the metadata for the <b>OtherUsers</b> folder.</summary>
        public static IKnownFolder OtherUsers
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.OtherUsers);
            }
        }

        /// <summary>Gets the metadata for the <b>PicturesLibrary</b> folder.</summary>
        public static IKnownFolder PicturesLibrary
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.PicturesLibrary);
            }
        }

        /// <summary>Gets the metadata for the <b>PublicRingtones</b> folder.</summary>
        public static IKnownFolder PublicRingtones
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.PublicRingtones);
            }
        }

        /// <summary>Gets the metadata for the <b>RecordedTVLibrary</b> folder.</summary>
        public static IKnownFolder RecordedTVLibrary
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.RecordedTVLibrary);
            }
        }

        /// <summary>Gets the metadata for the <b>Ringtones</b> folder.</summary>
        public static IKnownFolder Ringtones
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.Ringtones);
            }
        }

        /// <summary>
        ///Gets the metadata for the <b>UserPinned</b> folder.
        /// </summary>
        public static IKnownFolder UserPinned
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.UserPinned);
            }
        }

        /// <summary>Gets the metadata for the <b>UserProgramFiles</b> folder.</summary>
        public static IKnownFolder UserProgramFiles
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.UserProgramFiles);
            }
        }

        /// <summary>Gets the metadata for the <b>UserProgramFilesCommon</b> folder.</summary>
        public static IKnownFolder UserProgramFilesCommon
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.UserProgramFilesCommon);
            }
        }

        /// <summary>Gets the metadata for the <b>UsersLibraries</b> folder.</summary>
        public static IKnownFolder UsersLibraries
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.UsersLibraries);
            }
        }

        /// <summary>Gets the metadata for the <b>VideosLibrary</b> folder.</summary>
        public static IKnownFolder VideosLibrary
        {
            get
            {
                CoreHelpers.ThrowIfNotWin7();
                return GetKnownFolder(FolderIdentifiers.VideosLibrary);
            }
        }

        /// <summary>Gets the metadata for the <b>AddNewPrograms</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder AddNewPrograms
        {
            get
            {
                return GetKnownFolder(FolderIdentifiers.AddNewPrograms);
            }
        }

        /// <summary>Gets the metadata for the <b>AdminTools</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder AdminTools {
            get { return  GetKnownFolder(FolderIdentifiers.AdminTools);}}

        /// <summary>Gets the metadata for the <b>AppUpdates</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder AppUpdates {
            get { return  GetKnownFolder(FolderIdentifiers.AppUpdates);}}

        /// <summary>Gets the metadata for the <b>CDBurning</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CDBurning {
            get { return  GetKnownFolder(FolderIdentifiers.CDBurning);}}

        /// <summary>Gets the metadata for the <b>ChangeRemovePrograms</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ChangeRemovePrograms {
            get { return  GetKnownFolder(FolderIdentifiers.ChangeRemovePrograms);}}

        /// <summary>Gets the metadata for the <b>CommonAdminTools</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CommonAdminTools {
            get { return  GetKnownFolder(FolderIdentifiers.CommonAdminTools);}}

        /// <summary>Gets the metadata for the <b>CommonPrograms</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CommonPrograms {
            get { return  GetKnownFolder(FolderIdentifiers.CommonPrograms);}}

        /// <summary>Gets the metadata for the <b>CommonStartMenu</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CommonStartMenu {
            get { return  GetKnownFolder(FolderIdentifiers.CommonStartMenu);}}

        /// <summary>Gets the metadata for the <b>CommonStartup</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CommonStartup {
            get { return  GetKnownFolder(FolderIdentifiers.CommonStartup);}}

        /// <summary>Gets the metadata for the <b>CommonTemplates</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder CommonTemplates {
            get { return  GetKnownFolder(FolderIdentifiers.CommonTemplates);}}

        /// <summary>Gets the metadata for the <b>Computer</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Computer {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Computer);}}

        /// <summary>Gets the metadata for the <b>Conflict</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Conflict {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Conflict);}}

        /// <summary>Gets the metadata for the <b>Connections</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Connections {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Connections);}}

        /// <summary>Gets the metadata for the <b>Contacts</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Contacts {
            get { return  GetKnownFolder(FolderIdentifiers.Contacts);}}

        /// <summary>Gets the metadata for the <b>ControlPanel</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ControlPanel {
            get { return  GetKnownFolder(
                    FolderIdentifiers.ControlPanel);}}

        /// <summary>Gets the metadata for the <b>Cookies</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Cookies {
            get { return  GetKnownFolder(FolderIdentifiers.Cookies);}}

        /// <summary>Gets the metadata for the <b>Desktop</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Desktop {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Desktop);}}

        /// <summary>Gets the metadata for the per-user <b>Documents</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Documents {
            get { return  GetKnownFolder(FolderIdentifiers.Documents);}}

        /// <summary>Gets the metadata for the per-user <b>Downloads</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Downloads {
            get { return  GetKnownFolder(FolderIdentifiers.Downloads);}}

        /// <summary>Gets the metadata for the per-user <b>Favorites</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Favorites {
            get { return  GetKnownFolder(FolderIdentifiers.Favorites);}}

        /// <summary>Gets the metadata for the <b>Fonts</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Fonts {
            get { return  GetKnownFolder(FolderIdentifiers.Fonts);}}

        /// <summary>Gets the metadata for the <b>Games</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Games {
            get { return  GetKnownFolder(FolderIdentifiers.Games);}}

        /// <summary>Gets the metadata for the <b>GameTasks</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder GameTasks {
            get { return  GetKnownFolder(FolderIdentifiers.GameTasks);}}

        /// <summary>Gets the metadata for the <b>History</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder History {
            get { return  GetKnownFolder(FolderIdentifiers.History);}}

        /// <summary>Gets the metadata for the <b>Internet</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Internet {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Internet);}}

        /// <summary>Gets the metadata for the <b>InternetCache</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder InternetCache {
            get { return  GetKnownFolder(FolderIdentifiers.InternetCache);}}

        /// <summary>Gets the metadata for the per-user <b>Links</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Links {
            get { return  GetKnownFolder(FolderIdentifiers.Links);}}

        /// <summary>Gets the metadata for the per-user <b>LocalAppData</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder LocalAppData {
            get { return  GetKnownFolder(FolderIdentifiers.LocalAppData);}}

        /// <summary>Gets the metadata for the <b>LocalAppDataLow</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder LocalAppDataLow {
            get { return  GetKnownFolder(FolderIdentifiers.LocalAppDataLow);}}

        /// <summary>Gets the metadata for the <b>LocalizedResourcesDir</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder LocalizedResourcesDir {
            get { return  GetKnownFolder(FolderIdentifiers.LocalizedResourcesDir);}}

        /// <summary>Gets the metadata for the per-user <b>Music</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Music {
            get { return  GetKnownFolder(FolderIdentifiers.Music);}}

        /// <summary>Gets the metadata for the <b>NetHood</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder NetHood {
            get { return  GetKnownFolder(FolderIdentifiers.NetHood);}}

        /// <summary>Gets the metadata for the <b>Network</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Network {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Network);}}

        /// <summary>Gets the metadata for the <b>OriginalImages</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder OriginalImages {
            get { return  GetKnownFolder(FolderIdentifiers.OriginalImages);}}

        /// <summary>Gets the metadata for the <b>PhotoAlbums</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PhotoAlbums {
            get { return  GetKnownFolder(FolderIdentifiers.PhotoAlbums);}}

        /// <summary>Gets the metadata for the per-user <b>Pictures</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Pictures {
            get { return  GetKnownFolder(FolderIdentifiers.Pictures);}}

        /// <summary>Gets the metadata for the <b>Playlists</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Playlists {
            get { return  GetKnownFolder(FolderIdentifiers.Playlists);}}

        /// <summary>Gets the metadata for the <b>Printers</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Printers {
            get { return  GetKnownFolder(
                    FolderIdentifiers.Printers);}}

        /// <summary>Gets the metadata for the <b>PrintHood</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PrintHood {
            get { return  GetKnownFolder(FolderIdentifiers.PrintHood);}}

        /// <summary>Gets the metadata for the <b>Profile</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Profile {
            get { return  GetKnownFolder(FolderIdentifiers.Profile);}}

        /// <summary>Gets the metadata for the <b>ProgramData</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramData {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramData);}}

        /// <summary>Gets the metadata for the <b>ProgramFiles</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramFiles {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramFiles);}}

        /// <summary>Gets the metadata for the <b>ProgramFilesCommon</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramFilesCommon {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramFilesCommon);}}

        /// <summary>Gets the metadata for the <b>ProgramFilesCommonX64</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramFilesCommonX64 {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramFilesCommonX64);}}

        /// <summary>Gets the metadata for the <b>ProgramFilesCommonX86</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramFilesCommonX86 {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramFilesCommonX86);}}

        /// <summary>Gets the metadata for the <b>ProgramsFilesX64</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramFilesX64 {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramFilesX64);}}

        /// <summary>Gets the metadata for the <b>ProgramFilesX86</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ProgramFilesX86 {
            get { return  GetKnownFolder(FolderIdentifiers.ProgramFilesX86);}}

        /// <summary>Gets the metadata for the <b>Programs</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Programs {
            get { return  GetKnownFolder(FolderIdentifiers.Programs);}}

        /// <summary>Gets the metadata for the <b>Public</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Public {
            get { return  GetKnownFolder(FolderIdentifiers.Public);}}

        /// <summary>Gets the metadata for the <b>PublicDesktop</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicDesktop {
            get { return  GetKnownFolder(FolderIdentifiers.PublicDesktop);}}

        /// <summary>Gets the metadata for the <b>PublicDocuments</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicDocuments {
            get { return  GetKnownFolder(FolderIdentifiers.PublicDocuments);}}

        /// <summary>Gets the metadata for the <b>PublicDownloads</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicDownloads {
            get { return  GetKnownFolder(FolderIdentifiers.PublicDownloads);}}

        /// <summary>Gets the metadata for the <b>PublicGameTasks</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicGameTasks {
            get { return  GetKnownFolder(FolderIdentifiers.PublicGameTasks);}}

        /// <summary>Gets the metadata for the <b>PublicMusic</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicMusic {
            get { return  GetKnownFolder(FolderIdentifiers.PublicMusic);}}

        /// <summary>Gets the metadata for the <b>PublicPictures</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicPictures {
            get { return  GetKnownFolder(FolderIdentifiers.PublicPictures);}}

        /// <summary>Gets the metadata for the <b>PublicVideos</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder PublicVideos {
            get { return  GetKnownFolder(FolderIdentifiers.PublicVideos);}}

        /// <summary>Gets the metadata for the per-user <b>QuickLaunch</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder QuickLaunch {
            get { return  GetKnownFolder(FolderIdentifiers.QuickLaunch);}}

        /// <summary>Gets the metadata for the per-user <b>Recent</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Recent {
            get { return  GetKnownFolder(FolderIdentifiers.Recent);}}

        /// <summary>Gets the metadata for the <b>RecordedTV</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        /// <remarks>This folder is not used.</remarks>
        public static IKnownFolder RecordedTV {
            get { return  GetKnownFolder(FolderIdentifiers.RecordedTV);}}

        /// <summary>Gets the metadata for the <b>RecycleBin</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder RecycleBin {
            get { return  GetKnownFolder(
                    FolderIdentifiers.RecycleBin);}}

        /// <summary>Gets the metadata for the <b>ResourceDir</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder ResourceDir {
            get { return  GetKnownFolder(FolderIdentifiers.ResourceDir);}}

        /// <summary>Gets the metadata for the <b>RoamingAppData</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder RoamingAppData {
            get { return  GetKnownFolder(FolderIdentifiers.RoamingAppData);}}

        /// <summary>Gets the metadata for the <b>SampleMusic</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SampleMusic {
            get { return  GetKnownFolder(FolderIdentifiers.SampleMusic);}}

        /// <summary>Gets the metadata for the <b>SamplePictures</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SamplePictures {
            get { return  GetKnownFolder(FolderIdentifiers.SamplePictures);}}

        /// <summary>Gets the metadata for the <b>SamplePlaylists</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SamplePlaylists {
            get { return  GetKnownFolder(FolderIdentifiers.SamplePlaylists);}}

        /// <summary>Gets the metadata for the <b>SampleVideos</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SampleVideos {
            get { return  GetKnownFolder(FolderIdentifiers.SampleVideos);}}

        /// <summary>Gets the metadata for the per-user <b>SavedGames</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SavedGames {
            get { return  GetKnownFolder(FolderIdentifiers.SavedGames);}}

        /// <summary>Gets the metadata for the per-user <b>SavedSearches</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SavedSearches {
            get { return  GetKnownFolder(FolderIdentifiers.SavedSearches);}}

        /// <summary>Gets the metadata for the <b>SearchCsc</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SearchCsc {
            get { return  GetKnownFolder(FolderIdentifiers.SearchCsc);}}

        /// <summary>Gets the metadata for the <b>SearchHome</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SearchHome {
            get { return  GetKnownFolder(FolderIdentifiers.SearchHome);}}

        /// <summary>Gets the metadata for the <b>SearchMapi</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SearchMapi {
            get { return  GetKnownFolder(FolderIdentifiers.SearchMapi);}}

        /// <summary>Gets the metadata for the per-user <b>SendTo</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SendTo {
            get { return  GetKnownFolder(FolderIdentifiers.SendTo);}}

        /// <summary>Gets the metadata for the <b>SidebarDefaultParts</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SidebarDefaultParts {
            get { return  GetKnownFolder(FolderIdentifiers.SidebarDefaultParts);}}

        /// <summary>Gets the metadata for the <b>SidebarParts</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SidebarParts {
            get { return  GetKnownFolder(FolderIdentifiers.SidebarParts);}}

        /// <summary>Gets the metadata for the per-user <b>StartMenu</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder StartMenu {
            get { return  GetKnownFolder(FolderIdentifiers.StartMenu);}}

        /// <summary>Gets the metadata for the <b>Startup</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Startup {
            get { return  GetKnownFolder(FolderIdentifiers.Startup);}}

        /// <summary>Gets the metadata for the <b>SyncManager</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SyncManager {
            get { return  GetKnownFolder(
                    FolderIdentifiers.SyncManager);}}

        /// <summary>Gets the metadata for the <b>SyncResults</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SyncResults {
            get { return  GetKnownFolder(
                    FolderIdentifiers.SyncResults);}}

        /// <summary>Gets the metadata for the <b>SyncSetup</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SyncSetup {
            get { return  GetKnownFolder(
                    FolderIdentifiers.SyncSetup);}}

        /// <summary>Gets the metadata for the <b>System</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder System {
            get { return  GetKnownFolder(FolderIdentifiers.System);}}

        /// <summary>Gets the metadata for the <b>SystemX86</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder SystemX86 {
            get { return  GetKnownFolder(FolderIdentifiers.SystemX86);}}

        /// <summary>Gets the metadata for the <b>Templates</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Templates {
            get { return  GetKnownFolder(FolderIdentifiers.Templates);}}

        /// <summary>Gets the metadata for the <b>TreeProperties</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder TreeProperties {
            get { return  GetKnownFolder(FolderIdentifiers.TreeProperties);}}

        /// <summary>Gets the metadata for the <b>UserProfiles</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder UserProfiles {
            get { return  GetKnownFolder(FolderIdentifiers.UserProfiles);}}

        /// <summary>Gets the metadata for the <b>UsersFiles</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder UsersFiles {
            get { return  GetKnownFolder(FolderIdentifiers.UsersFiles);}}

        /// <summary>Gets the metadata for the <b>Videos</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Videos {
            get { return  GetKnownFolder(FolderIdentifiers.Videos);}}

        /// <summary>Gets the metadata for the <b>Windows</b> folder.</summary>
        /// <value>An <see cref="IKnownFolder"/> object.</value>
        public static IKnownFolder Windows {
            get { return  GetKnownFolder(FolderIdentifiers.Windows);}}

        private static ReadOnlyCollection<IKnownFolder> GetAllFolders()
        {
            // Should this method be thread-safe?? (It'll take a while to get a list of all the known folders, create the managed wrapper and
            // return the read-only collection.

            IList<IKnownFolder> foldersList = new List<IKnownFolder>();
            var folders = IntPtr.Zero;

            try
            {
                var knownFolderManager = new KnownFolderManagerClass();
                uint count;
                knownFolderManager.GetFolderIds(out folders, out count);

                if (count > 0 && folders != IntPtr.Zero)
                {
                    // Loop through all the KnownFolderID elements
                    for (var i = 0; i < count; i++)
                    {
                        // Read the current pointer
                        var current = new IntPtr(folders.ToInt64() + (Marshal.SizeOf(typeof(Guid)) * i));

                        // Convert to Guid
                        var knownFolderID = (Guid)Marshal.PtrToStructure(current, typeof(Guid));

                        var kf = KnownFolderHelper.FromKnownFolderIdInternal(knownFolderID);

                        // Add to our collection if it's not null (some folders might not exist on the system or we could have an exception
                        // that resulted in the null return from above method call
                        if (kf != null) { foldersList.Add(kf); }
                    }
                }
            }
            finally
            {
                if (folders != IntPtr.Zero) { Marshal.FreeCoTaskMem(folders); }
            }

            return new ReadOnlyCollection<IKnownFolder>(foldersList);
        }

        private static IKnownFolder GetKnownFolder(Guid guid)
        {
            return KnownFolderHelper.FromKnownFolderId(guid);
        }
    }
}