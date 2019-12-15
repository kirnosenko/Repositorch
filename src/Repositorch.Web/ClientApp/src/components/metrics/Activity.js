import React, { Component } from 'react';
import Loading from '../Loading';

export class Activity extends Component {
    static displayName = Activity.name;

    constructor(props) {
        super(props);
        this.state = { data: null };
    }

    componentDidMount() {
        this.populateData();
    }

    static renderData(data) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Period</th>
                        <th>Commits (total)</th>
                        <th>Authors (total)</th>
                        <th>Files</th>
                        <th>Added LOC (total)</th>
                        <th>Removed LOC (total)</th>
                        <th>LOC</th>
                    </tr>
                </thead>
                <tbody>
                    {data.periods.map(period =>
                        <tr key={period.name}>
                            <td>{period.title}</td>
                            <td>{period.commits}</td>
                            <td>{period.authors}</td>
                            <td>{period.files}</td>
                            <td>{period.locAdded}</td>
                            <td>{period.locRemoved}</td>
                            <td>{period.loc}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.data === null
            ? <Loading />
            : Activity.renderData(this.state.data);

        return (
            <div>
                <h2 id="tabelLabel">{Activity.displayName}</h2>
                {contents}
            </div>
        );
    }

    async populateData() {
        const response = await fetch('api/data/CalculateMetrics/Activity');
        const data = await response.json();
        this.setState({ data: data });
    }
}
