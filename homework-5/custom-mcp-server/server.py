from pathlib import Path
from fastmcp import FastMCP

mcp = FastMCP("lorem-ipsum-server")

LOREM_FILE = Path(__file__).parent / "lorem-ipsum.md"


@mcp.resource("lorem://text/{word_count}")
def lorem_resource(word_count: int = 30) -> str:
    """Returns the first word_count words from lorem-ipsum.md."""
    text = LOREM_FILE.read_text()
    words = text.split()
    return " ".join(words[:word_count])


@mcp.tool()
def read(word_count: int = 30) -> str:
    """Read the first word_count words from the lorem ipsum text (default: 30)."""
    text = LOREM_FILE.read_text()
    words = text.split()
    return " ".join(words[:word_count])


if __name__ == "__main__":
    mcp.run()
