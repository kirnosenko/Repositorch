import React, { Component } from 'react';

export class Files extends Component {
    static displayName = Files.name;

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
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>Extension</th>
                            <th>Number of files (%)</th>
                            <th>Added LOC</th>
                            <th>Removed LOC</th>
                            <th>Remain LOC</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.exts.map(ext =>
                            <tr key={ext.name}>
                                <td>{ext.name}</td>
                                <td>{ext.files}</td>
                                <td>{ext.addedLoc}</td>
                                <td>{ext.removedLoc}</td>
                                <td>{ext.remainLoc}</td>
                            </tr>
                        )}
                    </tbody>
                </table>
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>Directory</th>
                            <th>Number of files (%)</th>
                            <th>Added LOC</th>
                            <th>Removed LOC</th>
                            <th>Remain LOC</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.dirs.map(dir =>
                            <tr key={dir.name}>
                                <td>{dir.name}</td>
                                <td>{dir.files}</td>
                                <td>{dir.addedLoc}</td>
                                <td>{dir.removedLoc}</td>
                                <td>{dir.remainLoc}</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Files.renderData(this.state.data);

        return (
            <div>
                <h2 id="tabelLabel">{Files.displayName}</h2>
                {contents}
            </div>
        );
    }

    async populateData() {
        const response = await fetch('api/data/CalculateMetrics/Files');
        const data = await response.json();
        this.setState({ data: data, loading: false });
    }
}
