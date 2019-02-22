using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;

namespace Own.BlockchainExplorer.Api.Common
{
    public static class ControllerBaseExtensions
    {
        public static IEnumerable<Alert> GetValidationErrors(this ControllerBase controller)
        {
            return controller.ModelState.Values
                .SelectMany(v => v.Errors
                    .Select(e => Alert.Error(
                        Debugger.IsAttached && e.ErrorMessage.IsNullOrWhiteSpace()
                            ? e.Exception?.LogFormat(true)
                            : e.ErrorMessage)));
        }
    }
}
