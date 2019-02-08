﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Core.Impl {
    public class CoinGroupSet : ICoinGroupSet {
        private readonly Dictionary<Guid, CoinGroupData> _dicById = new Dictionary<Guid, CoinGroupData>();

        private readonly INTMinerRoot _root;
        public CoinGroupSet(INTMinerRoot root) {
            _root = root;
            VirtualRoot.Access<RefreshCoinGroupSetCommand>(
                Guid.Parse("6597A41B-A7C9-402E-A4F9-7642FA185313"),
                "处理刷新币组数据集命令",
                LogEnum.Console,
                action: message => {
                    var repository = NTMinerRoot.CreateServerRepository<CoinGroupData>();
                    foreach (var item in repository.GetAll()) {
                        if (_dicById.ContainsKey(item.Id)) {
                            VirtualRoot.Execute(new UpdateCoinGroupCommand(item));
                        }
                        else {
                            VirtualRoot.Execute(new AddCoinGroupCommand(item));
                        }
                    }
                });
            VirtualRoot.Access<AddCoinGroupCommand>(
                Guid.Parse("2dd8f7e9-c79d-4621-954f-9fc45b0207dd"),
                "添加币组",
                LogEnum.Console,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    CoinGroupData entity = new CoinGroupData().Update(message.Input);
                    if (ReferenceEquals(entity, message.Input)) {
                        return;
                    }
                    _dicById.Add(entity.Id, entity);
                    var repository = NTMinerRoot.CreateServerRepository<CoinGroupData>();
                    repository.Add(entity);

                    VirtualRoot.Happened(new CoinGroupAddedEvent(entity));
                });
            VirtualRoot.Access<RemoveCoinGroupCommand>(
                Guid.Parse("e52874f4-37d8-4d49-a637-5b95aa89367e"),
                "移除币组",
                LogEnum.Console,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.EntityId == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (!_dicById.ContainsKey(message.EntityId)) {
                        return;
                    }
                    CoinGroupData entity = _dicById[message.EntityId];
                    _dicById.Remove(entity.GetId());
                    var repository = NTMinerRoot.CreateServerRepository<CoinGroupData>();
                    repository.Remove(message.EntityId);

                    VirtualRoot.Happened(new CoinGroupRemovedEvent(entity));
                });
        }

        private bool _isInited = false;
        private object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            lock (_locker) {
                if (!_isInited) {
                    var repository = NTMinerRoot.CreateServerRepository<CoinGroupData>();
                    foreach (var item in repository.GetAll()) {
                        if (!_dicById.ContainsKey(item.GetId())) {
                            _dicById.Add(item.GetId(), item);
                        }
                    }
                    _isInited = true;
                }
            }
        }

        public List<Guid> GetGroupCoinIds(Guid groupId) {
            InitOnece();
            return _dicById.Values.Where(a => a.GroupId == groupId).Select(a => a.CoinId).ToList();
        }

        public IEnumerator<ICoinGroup> GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }
    }
}
