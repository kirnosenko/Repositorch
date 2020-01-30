import React, { Component } from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import ProjectList from './components/projects/ProjectList';
import ProjectBrowse from './components/projects/ProjectBrowse';
import CreateProject from './components/projects/CreateProject';
import './custom.css'

export default class App extends Component {
	static displayName = App.name;

	render () {
		return (
			<Layout>
				<Route exact path='/' component={ProjectList} />
				<Route exact path='/new' component={CreateProject} />
				<Route exact path='/:project' component={ProjectBrowse} />
				<Route exact path='/:project/:metric' component={ProjectBrowse} />
			</Layout>
		);
	}
}
