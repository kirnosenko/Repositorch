import React from 'react';
import { Redirect } from 'react-router-dom'

export default function CreateProject() {
    const [state, setState] = React.useState({
        name: "",
        repositoryPath: "",
        branch: "master",
        useExtendedLog: true,
        checkResult: "1"
    });

    function handleChange(evt) {
        const value = evt.target.type === "checkbox"
            ? evt.target.checked
            : evt.target.value;
        setState({
            ...state,
            [evt.target.name]: value
        });
    }

    function handleSubmit(event) {
        event.preventDefault();
        
        fetch("api/Projects/Create", {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(state)
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            setState(null);
        })
        .catch((error) => {
            alert(error);
        });
    }

    if (state == null) {
        return <Redirect to='/' />
    }

    return (
        <div>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <div className="heading">Project name</div>
                    <input
                        className="form-control"
                        type="text"
                        name="name"
                        value={state.name}
                        onChange={handleChange} />
                    <small className="form-text text-muted">
                        May consist of letters(A-Z, a-z), digits (0-9) and special characters '-', '.', '_', '~'.
                    </small>
                </div>
                <div className="form-group">
                    <div className="heading">Repository path</div>
                    <input
                        className="form-control"
                        type="text"
                        name="repositoryPath"
                        value={state.repositoryPath}
                        onChange={handleChange} />
                    <small className="form-text text-muted">
                        Path to the repository to work with. Only git repositories are supported at the moment.
                    </small>
                </div>
                <div className="form-group">
                    <div className="heading">Repository branch</div>
                    <input
                        className="form-control"
                        type="text"
                        name="branch"
                        value={state.branch}
                        onChange={handleChange} />
                    <small className="form-text text-muted">
                        Branch in the repository to work with. 'master' for git in most cases.
                    </small>
                </div>
                <div className="form-group form-check">
                    <input
                        type="checkbox"
                        className="form-check-input"
                        name="useExtendedLog"
                        checked={state.useExtendedLog}
                        onChange={handleChange} />
                    <label className="form-check-label">Use extended log</label>
                    <small className="form-text text-muted">
                        Don't touch if you are not sure.
                    </small>
                </div>
                <div className="form-group">
                    <div className="heading">Check result</div>
                    <select
                        className="custom-select"
                        name="checkResult"
                        value={state.checkResult}
                        onChange={handleChange}>
                        <option value="0">Nothing</option>
                        <option value="1">Modified</option>
                        <option value="2">Everything</option>
                    </select>
                    <small className="form-text text-muted">
                        Don't touch if you are not sure.
                    </small>
                </div>
                <button type='submit' className="btn btn-primary">
                    Create project...
                </button>
            </form>
        </div>
    );
}
