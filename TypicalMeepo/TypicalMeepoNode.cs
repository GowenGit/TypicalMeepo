﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meepo;
using Meepo.Core.Configs;
using Meepo.Core.Extensions;
using Meepo.Core.StateMachine;
using TypicalMeepo.Core.Events;
using TypicalMeepo.Core.Extensions;

namespace TypicalMeepo
{
    public class TypicalMeepoNode : ITypicalMeepoNode
    {
        private readonly IMeepoNode meepo;

        private readonly Dictionary<Type, MessageReceivedHandler> handlers = new Dictionary<Type, MessageReceivedHandler>();

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        public TypicalMeepoNode(TcpAddress listenerAddress)
        {
            meepo = new MeepoNode(listenerAddress);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="config">Meepo configuration</param>
        public TypicalMeepoNode(TcpAddress listenerAddress, MeepoConfig config)
        {
            meepo = new MeepoNode(listenerAddress, config);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="serverAddresses">List of server addresses to connect to</param>
        public TypicalMeepoNode(TcpAddress listenerAddress, IEnumerable<TcpAddress> serverAddresses)
        {
            meepo = new MeepoNode(listenerAddress, serverAddresses);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="serverAddresses">List of server addresses to connect to</param>
        /// <param name="config">Meepo configuration</param>
        public TypicalMeepoNode(TcpAddress listenerAddress, IEnumerable<TcpAddress> serverAddresses, MeepoConfig config)
        {
            meepo = new MeepoNode(listenerAddress, serverAddresses, config);
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="meepo">Fake Meepo node</param>
        internal TypicalMeepoNode(IMeepoNode meepo)
        {
            this.meepo = meepo ?? throw new ArgumentNullException(nameof(meepo));
        }

        #endregion

        /// <summary>
        /// Run Meepo server.
        /// Starts listening for new clients
        /// and connects to specified servers.
        /// </summary>
        public void Start()
        {
            meepo.Start();
        }

        /// <summary>
        /// Stop Meepo server.
        /// </summary>
        public void Stop()
        {
            meepo.Stop();
        }

        /// <summary>
        /// Remove client.
        /// </summary>
        /// <param name="id">Client ID</param>
        public void RemoveClient(Guid id)
        {
            meepo.RemoveClient(id);
        }

        /// <summary>
        /// Send data to a specific client.
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        public async Task SendAsync<T>(Guid id, T data)
        {
            await meepo.SendAsync(id, typeof(T).GetAssemblyName().Encode());

            await meepo.SendAsync(id, data.PackageToBytes());
        }

        /// <summary>
        /// Send data to a all clients.
        /// Including connected servers.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        public async Task SendAsync<T>(T data)
        {
            await meepo.SendAsync(typeof(T).GetAssemblyName().Encode());

            await meepo.SendAsync(data.PackageToBytes());
        }

        /// <summary>
        /// Meepo server state.
        /// </summary>
        public State ServerState => meepo.ServerState;

        /// <summary>
        /// Get IDs and addresses of all connected servers.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            return (Dictionary<Guid, TcpAddress>) meepo.GetServerClientInfos();
        }

        /// <summary>
        /// Subscribe to event when message of a specific type is received.
        /// </summary>
        /// <typeparam name="T">Meepo package</typeparam>
        /// <param name="action">MessageReceivedHandler delegate</param>
        public void Subscribe<T>(MessageReceivedHandler<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var subscriber = new MessageReceivedSubscriber<T>(action);

            var type = typeof(T);

            if (handlers.ContainsKey(type)) meepo.MessageReceived -= handlers[type];

            handlers[type] = subscriber.OnMessageReceived;

            meepo.MessageReceived += handlers[type];
        }

        /// <summary>
        /// Unsubscribe from event.
        /// </summary>
        /// <typeparam name="T">Meepo package</typeparam>
        public void Unsubscribe<T>()
        {
            if (!handlers.ContainsKey(typeof(T))) return;

            meepo.MessageReceived -= handlers[typeof(T)];
        }

        /// <summary>
        /// Stop Meepo server and dispose.
        /// </summary>
        public void Dispose()
        {
            meepo.Stop();
        }
    }
}
