import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Summary } from './components/metrics/Summary';
import { Authors } from './components/metrics/Authors';
import { Files } from './components/metrics/Files';
import { Activity } from './components/metrics/Activity';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/summary' component={Summary} />
        <Route path='/authors' component={Authors} />
        <Route path='/files' component={Files} />
        <Route path='/activity' component={Activity} />
      </Layout>
    );
  }
}
