import { expect as expectCDK, matchTemplate, MatchStyle } from '@aws-cdk/assert';
import * as cdk from '@aws-cdk/core';
import * as HostedZones from '../lib/hosted_zones-stack';

test('Empty Stack', () => {
    const app = new cdk.App();
    // WHEN
    const stack = new HostedZones.HostedZonesStack(app, 'MyTestStack');
    // THEN
    expectCDK(stack).to(matchTemplate({
      "Resources": {}
    }, MatchStyle.EXACT))
});
