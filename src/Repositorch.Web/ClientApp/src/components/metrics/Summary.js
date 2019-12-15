import React, { Component } from 'react';

export class Summary extends Component {
    static displayName = Summary.name;

    constructor(props) {
        super(props);
        this.state = { data: {}, loading: true };
    }

    componentDidMount() {
        this.populateData();
    }

    static renderData(data) {
        return (
            <div>
                <br/>Statistics period: from {data.periodFrom} to {data.periodTo} ({data.periodDays} days, {data.periodYears} years)
                <br/>Number of authors: {data.authors}
                <br/>Number of commits: {data.commits}
                <br/>Number of fix commits: {data.commitsFix} ({data.commitsFixPercent} %)
                <br/>Number of refactoring commits: {data.commitsRefactoring} ({data.commitsRefactoringPercent} %)
                <br/>Number of files: {data.files} ({data.filesAdded} added, {data.filesRemoved} removed)
                <br/>Number of LOC: {data.loc} ({data.locAdded} added, {data.locRemoved} removed)
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Summary.renderData(this.state.data);

        return (
            <div>
                <h2>Summary</h2>
                {contents}
            </div>
        );
    }

    async populateData() {
        const response = await fetch('api/data/CalculateMetrics/Summary');
        const data = await response.json();
        this.setState({ data: data, loading: false });
    }
}
