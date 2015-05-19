using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface ISettingsViewModel {

    }

    public class SettingsViewModel :ViewModelBase, ISettingsViewModel {

        public SettingsViewModel(IScreen screen)
            : base(screen) {

            var match = Regex.Match(Config.Instance.ConnectionString, @"server=([^;]+);database([^;]+);", RegexOptions.IgnoreCase);
            if (match.Success) {
                this.ServerName = match.Groups[1].Value;
                this.DatabaseName = match.Groups[2].Value;
            }
            match = Regex.Match(Config.Instance.ConnectionString, @"data source=([^;]+);initial catalog=([^;]+);", RegexOptions.IgnoreCase);
            if (match.Success) {
                this.ServerName = match.Groups[1].Value;
                this.DatabaseName = match.Groups[2].Value;
            }
        }

        string _databaseName;
        public string DatabaseName {
            get { return _databaseName; }
            set { this.RaiseAndSetIfChanged(ref _databaseName, value); }
        }

        string _serverName;
        public string ServerName {
            get { return _serverName; }
            set { this.RaiseAndSetIfChanged(ref _serverName, value); }
        }
    }
}
