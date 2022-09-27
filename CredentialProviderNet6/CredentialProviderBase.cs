﻿using CredProvider.NET.Interop2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static CredProvider.NET.Constants;

namespace CredProvider.NET
{
    public abstract class CredentialProviderBase : ICredentialProvider, ICredentialProviderSetUserArray
    {
        private ICredentialProviderEvents? events;

        protected abstract CredentialView Initialize(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags);

        private CredentialView? view;
        private _CREDENTIAL_PROVIDER_USAGE_SCENARIO usage;

        private readonly List<ICredentialProviderUser> providerUsers = new();

        public virtual int SetUsageScenario(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags)
        {
            view = Initialize(cpus, dwFlags);
            usage = cpus;

            if (view.Active)
            {
                return HRESULT.S_OK;
            }

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetSerialization(ref _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION pcpcs)
        {
            Logger.Write($"ulAuthenticationPackage: {pcpcs.ulAuthenticationPackage}");

            return HRESULT.S_OK;
        }

        public virtual int Advise(ICredentialProviderEvents pcpe, ulong upAdviseContext)
        {
            Logger.Write($"upAdviseContext: {upAdviseContext}");

            if (pcpe != null)
            {
                events = pcpe;

                Marshal.AddRef(Marshal.GetIUnknownForObject(pcpe));
            }

            return HRESULT.S_OK;
        }

        public virtual int UnAdvise()
        {
            Logger.Write();

            if (events != null)
            {
                //Marshal.Release(Marshal.GetIUnknownForObject(events));
                events = null;
            }

            return HRESULT.S_OK;
        }

        public virtual int GetFieldDescriptorCount(out uint pdwCount)
        {
            Logger.Write();

            pdwCount = (uint)(view?.DescriptorCount ?? 0);

            Logger.Write($"Returning field count: {pdwCount}");

            return HRESULT.S_OK;
        }

        public virtual int GetFieldDescriptorAt(uint dwIndex, [Out] IntPtr ppcpfd)
        {
            if (view?.GetField((int)dwIndex, ppcpfd) ?? false)
            {
                return HRESULT.S_OK;
            }

            return HRESULT.E_INVALIDARG;
        }

        public virtual int GetCredentialCount(
            out uint pdwCount,
            out uint pdwDefault,
            out int pbAutoLogonWithDefault
        )
        {
            Logger.Write();

            pdwCount = (uint)(view?.CredentialCount ?? 0);

            pdwDefault = (uint)(view?.DefaultCredential ?? 0);

            pbAutoLogonWithDefault = 0;

            return HRESULT.S_OK;
        }

        public virtual int GetCredentialAt(uint dwIndex, out ICredentialProviderCredential? ppcpc)
        {
            Logger.Write($"dwIndex: {dwIndex}");

            ppcpc = view?.CreateCredential((int)dwIndex);

            return HRESULT.S_OK;
        }

        public virtual _CREDENTIAL_PROVIDER_USAGE_SCENARIO GetUsage()
        {
            return usage;
        }

        public virtual int SetUserArray(ICredentialProviderUserArray users)
        {
            this.providerUsers.Clear();

            users.GetCount(out uint count);
            users.GetAccountOptions(out CREDENTIAL_PROVIDER_ACCOUNT_OPTIONS options);

            Logger.Write($"count: {count}; options: {options}");

            for (uint i = 0; i < count; i++)
            {
                users.GetAt(i, out ICredentialProviderUser user);

                user.GetProviderID(out Guid providerId);
                user.GetSid(out string sid);

                this.providerUsers.Add(user);

                Logger.Write($"providerId: {providerId}; sid: {sid}");
            }

            Logger.Write($"Set providerUsers to array with length {this.providerUsers.Count}");

            return HRESULT.S_OK;
        }

        //Lookup the user by index and return the sid
        public virtual string GetUserSid(int dwIndex)
        {
            Logger.Write($"Total users: {providerUsers.Count}");
            string sid = string.Empty;

            //CredUI does not provide user sids, so return null
            if (this.providerUsers.Count < dwIndex + 1)
            {
                Logger.Write($"Not enough users in the array, total {providerUsers.Count} users, asking for #{dwIndex}");
                return sid;
            }

            try
            {
                this.providerUsers[dwIndex].GetSid(out sid);
                Logger.Write($"returning user {sid}");
            }
            catch (Exception e)
            {
                Logger.Write($"Exception getting user {dwIndex}: {e.Message}");
            }
            
            return sid;
        }
    }
}
