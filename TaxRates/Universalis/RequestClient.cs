using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxRates.Serialized;

namespace TaxRates.Universalis
{
    public static class RequestClient
    {
        public static async Task<RatesResponse> GetTaxRates(string selectedWorld, CancellationToken cancellationToken)
        {
            var uri = new UriBuilder($"https://universalis.app/api/tax-rates?world={selectedWorld}");

            cancellationToken.ThrowIfCancellationRequested();

            var client = new HttpClient();
            var response = await client.GetStreamAsync(uri.Uri, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var parsResponse = await JsonSerializer.DeserializeAsync<RatesResponse>(response, cancellationToken: cancellationToken).ConfigureAwait(false);

            return parsResponse;
        }
    }
}
