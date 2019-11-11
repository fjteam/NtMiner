﻿using NTMiner.Core;
using NTMiner.Vms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

namespace NTMiner {
    public partial class AppContext {
        public class NTMinerWalletViewModels : ViewModelBase, IEnumerable<NTMinerWalletViewModel> {
            public static readonly NTMinerWalletViewModels Instance = new NTMinerWalletViewModels();
            private readonly Dictionary<Guid, NTMinerWalletViewModel> _dicById = new Dictionary<Guid, NTMinerWalletViewModel>();

            public ICommand Add { get; private set; }

            private NTMinerWalletViewModels() {
#if DEBUG
                Write.Stopwatch.Start();
#endif
                if (WpfUtil.IsInDesignMode) {
                    return;
                }
                Init(refresh: false);
                AppContextEventPath<NTMinerWalletSetInitedEvent>("NTMiner钱包集初始化后", LogEnum.DevConsole,
                    action: message => {
                        Init(refresh: true);
                    });
                this.Add = new DelegateCommand(() => {
                    new NTMinerWalletViewModel(Guid.NewGuid()).Edit.Execute(FormType.Add);
                });
                AppContextEventPath<NTMinerWalletAddedEvent>("添加NTMiner钱包后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        if (!_dicById.ContainsKey(message.Source.GetId())) {
                            _dicById.Add(message.Source.GetId(), new NTMinerWalletViewModel(message.Source));
                            if (AppContext.Instance.CoinVms.TryGetCoinVm(message.Source.CoinId, out CoinViewModel coinVm)) {
                                coinVm.OnPropertyChanged(nameof(coinVm.NTMinerWallets));
                            }
                        }
                    });
                AppContextEventPath<NTMinerWalletUpdatedEvent>("更新NTMiner钱包后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        _dicById[message.Source.GetId()].Update(message.Source);
                    });
                AppContextEventPath<NTMinerWalletRemovedEvent>("删除NTMiner钱包后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        _dicById.Remove(message.Source.GetId());
                        if (AppContext.Instance.CoinVms.TryGetCoinVm(message.Source.CoinId, out CoinViewModel coinVm)) {
                            coinVm.OnPropertyChanged(nameof(coinVm.NTMinerWallets));
                        }
                    });
#if DEBUG
                var elapsedMilliseconds = Write.Stopwatch.Stop();
                Write.DevTimeSpan($"耗时{elapsedMilliseconds}毫秒 {this.GetType().Name}.ctor");
#endif
            }

            private void Init(bool refresh) {
                _dicById.Clear();
                foreach (var item in NTMinerRoot.Instance.NTMinerWalletSet) {
                    _dicById.Add(item.GetId(), new NTMinerWalletViewModel(item));
                }
                if (refresh) {
                    foreach (var coinVm in AppContext.Instance.CoinVms.AllCoins) {
                        coinVm.OnPropertyChanged(nameof(coinVm.NTMinerWallets));
                    }
                }
            }

            public bool TryGetMineWorkVm(Guid id, out NTMinerWalletViewModel ntMinerWalletVm) {
                return _dicById.TryGetValue(id, out ntMinerWalletVm);
            }

            public IEnumerator<NTMinerWalletViewModel> GetEnumerator() {
                return _dicById.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _dicById.Values.GetEnumerator();
            }
        }
    }
}