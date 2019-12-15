import React, { Component } from 'react';

export class Authors extends Component {
    static displayName = Authors.name;

    constructor(props) {
        super(props);
        this.state = { data: {}, loading: true };
    }

    componentDidMount() {
        this.populateData();
    }

    static renderData(data) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Author</th>
                        <th>Commits (%)</th>
                        <th>Fix commits (%)</th>
                        <th>Refactoring commits (%)</th>
                        <th>Added LOC</th>
                        <th>Removed LOC</th>
                        <th>Remain LOC</th>
                        <th>Contribution %</th>
                        <th>Specialization %</th>
                        <th>Unique specialization %</th>
                        <th>Demand for code %</th>
                    </tr>
                </thead>
                <tbody>
                    {data.authors.map(author =>
                        <tr key={author.name}>
                            <td>{author.name}</td>
                            <td>{author.commits}</td>
                            <td>{author.fixes}</td>
                            <td>{author.refactorings}</td>
                            <td>{author.addedLoc}</td>
                            <td>{author.removedLoc}</td>
                            <td>{author.remainLoc}</td>
                            <td>{author.contribution}</td>
                            <td>{author.specialization}</td>
                            <td>{author.uniqueSpecialization}</td>
                            <td>{author.demandForCode}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Authors.renderData(this.state.data);

        return (
            <div>
                <h2 id="tabelLabel">{Authors.displayName}</h2>
                {contents}
            </div>
        );
    }

    async populateData() {
        const response = await fetch('api/data/CalculateMetrics/Authors');
        const data = await response.json();
        this.setState({ data: data, loading: false });
    }
}
