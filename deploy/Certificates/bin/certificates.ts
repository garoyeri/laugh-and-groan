#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import { CertificatesStack } from '../lib/certificates-stack';

const app = new cdk.App();
new CertificatesStack(app, 'CertificatesStack');
