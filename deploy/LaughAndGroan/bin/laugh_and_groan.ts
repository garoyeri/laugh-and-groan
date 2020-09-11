#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import { LaughAndGroanStack } from '../lib/laugh_and_groan-stack';

const app = new cdk.App();
new LaughAndGroanStack(app, 'LaughAndGroanStack', {
  env: {
    region: process.env.AWS_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT,
  }
});
