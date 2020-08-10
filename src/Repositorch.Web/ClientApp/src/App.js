import React, { Component } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Layout } from './components/Layout';
import ProjectList from './components/projects/ProjectList';
import ProjectBrowse from './components/projects/ProjectBrowse';
import ProjectEdit from './components/projects/ProjectEdit';
import Environment from './components/Environment';
import './custom.css';

export default class App extends Component {
	static displayName = App.name;

	render () {
		return (
			<Layout>
				<Switch>
					<Route exact path='/' component={ProjectList} />
					<Route exact path='/env' component={Environment} />
					<Route exact path='/new' component={ProjectEdit} />
					<Route exact path='/edit/:project' component={ProjectEdit} />
					<Route exact path='/:project' component={ProjectBrowse} />
					<Route exact path='/:project/*' component={ProjectBrowse} />
				</Switch>
			</Layout>
		);
	}
}
