import { Component, type ErrorInfo, type ReactNode } from "react";
import styles from "./ErrorBoundary.module.css";

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  error: Error | null;
}

// Without this, an uncaught render error (e.g. rendering a field that
// doesn't exist on an unexpected API response shape) unmounts the whole
// React tree and leaves a blank white page with no indication anything
// went wrong. This catches that and shows something actionable instead.
export default class ErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Unhandled error in render tree:", error, info.componentStack);
  }

  handleReload = () => {
    this.setState({ error: null });
    window.location.reload();
  };

  render() {
    if (this.state.error) {
      return (
        <div className={styles.container}>
          <div className={styles.card}>
            <h1>Something went wrong</h1>

            <p>
              This page hit an unexpected error and couldn't finish
              rendering.
            </p>

            {import.meta.env.DEV && (
              <pre className={styles.details}>{this.state.error.message}</pre>
            )}

            <button
              type="button"
              className={styles.button}
              onClick={this.handleReload}
            >
              Reload page
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
