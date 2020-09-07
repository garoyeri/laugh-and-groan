import { expect as expectCDK, matchTemplate, MatchStyle } from '@aws-cdk/assert';
import * as cdk from '@aws-cdk/core';
import * as LaughAndGroan from '../lib/laugh_and_groan-stack';

test('Empty Stack', () => {
    const app = new cdk.App();
    // WHEN
    const stack = new LaughAndGroan.LaughAndGroanStack(app, 'MyTestStack');
    // THEN
    expectCDK(stack).to(matchTemplate({
      "Resources": {}
    }, MatchStyle.EXACT))
});
